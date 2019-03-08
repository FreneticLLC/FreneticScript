//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

#if DEBUG
#define VALIDATE
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.ScriptSystems
{
    /// <summary>
    /// Represents one CIL adapter variable.
    /// </summary>
    public class SingleCILVariable
    {
        /// <summary>
        /// The index of the local variable.
        /// </summary>
        public int Index;

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the variable.
        /// </summary>
        public TagType Type;

        /// <summary>
        /// Constructs a single CIL adapter variable.
        /// </summary>
        /// <param name="_index">The variable index.</param>
        /// <param name="_name">The variable name.</param>
        /// <param name="_type">The variable type.</param>
        public SingleCILVariable(int _index, string _name, TagType _type)
        {
            Index = _index;
            Name = _name;
            Type = _type;
        }
    }

    /// <summary>
    /// Holds all data needed for CIL adaptation.
    /// </summary>
    public class CILAdaptationValues
    {
        /// <summary>
        /// The compiled CSE involved.
        /// </summary>
        public CompiledCommandStackEntry Entry;

        /// <summary>
        /// The compiling script.
        /// </summary>
        public CommandScript Script;

        /// <summary>
        /// The debug mode currently in use.
        /// </summary>
        public DebugMode DBMode;

        /// <summary>
        /// Gets the command at the index.
        /// </summary>
        /// <param name="index">The index value.</param>
        /// <returns>The command.</returns>
        public CommandEntry CommandAt(int index)
        {
            return Entry.Entries[index];
        }

        /// <summary>
        /// Represents the <see cref="CommandStackEntry.Entries"/> field.
        /// </summary>
        public static readonly FieldInfo EntriesField = typeof(CommandStackEntry).GetField(nameof(CommandStackEntry.Entries));

        /// <summary>
        /// Represents the <see cref="CommandEntry.Command"/> field.
        /// </summary>
        public static readonly FieldInfo Entry_CommandField = typeof(CommandEntry).GetField(nameof(CommandEntry.Command));

        /// <summary>
        /// Represents the <see cref="CommandEntry.GetArgumentObject(CommandQueue, int)"/> method.
        /// </summary>
        public static readonly MethodInfo Entry_GetArgumentObjectMethod = typeof(CommandEntry).GetMethod(nameof(CommandEntry.GetArgumentObject), new Type[] { typeof(CommandQueue), typeof(int) });

        /// <summary>
        /// Represents the <see cref="CommandEntry.Arguments"/> field.
        /// </summary>
        public static readonly FieldInfo Entry_ArgumentsField = typeof(CommandEntry).GetField(nameof(CommandEntry.Arguments));

        /// <summary>
        /// Represents the <see cref="IntHolder.Internal"/> field.
        /// </summary>
        public static readonly FieldInfo IntHolder_InternalField = typeof(IntHolder).GetField(nameof(IntHolder.Internal));

        /// <summary>
        /// Represents the <see cref="CommandQueue.SetLocalVar(int, TemplateObject)"/> method.
        /// </summary>
        public static readonly MethodInfo Queue_SetLocalVarMethod = typeof(CommandQueue).GetMethod(nameof(CommandQueue.SetLocalVar), new Type[] { typeof(int), typeof(TemplateObject) });

        /// <summary>
        /// Represents the <see cref="CommandQueue.Error"/> field.
        /// </summary>
        public static readonly FieldInfo Queue_Error = typeof(CommandQueue).GetField(nameof(CommandQueue.Error));

        /// <summary>
        /// The type of the class <see cref="CommandEntry"/> class.
        /// </summary>
        public static readonly Type CommandEntryType = typeof(CommandEntry);

        /// <summary>
        /// Tracks generated IL.
        /// </summary>
        public class ILGeneratorTracker
        {
            /// <summary>
            /// Internal generator.
            /// </summary>
            public ILGenerator Internal;

            /// <summary>
            /// The backing system.
            /// </summary>
            public ScriptEngine System;

            /// <summary>
            /// All codes generated. Only has a value when compiled in DEBUG mode.
            /// </summary>
            public List<KeyValuePair<string, object>> Codes
#if DEBUG
                = new List<KeyValuePair<string, object>>()
#endif
                ;

            /// <summary>
            /// Stack size tracker, for validation.
            /// </summary>
            public int StackSize = 0;

            /// <summary>
            /// Gives a warning if stack size is not the exact correct size.
            /// </summary>
            /// <param name="situation">The current place in the code that requires a validation.</param>
            /// <param name="expected">The expected stack size.</param>
            [Conditional("VALIDATE")]
            public void ValidateStackSizeIs(string situation, int expected)
            {
                if (StackSize != expected)
                {
                    System.Context.BadOutput("Stack not well sized at " + TextStyle.Separate + situation + TextStyle.Base
                        + "... size = " + TextStyle.Separate + StackSize + TextStyle.Base
                        + " but should be exactly " + TextStyle.Separate + expected + TextStyle.Base + " for code:\n"
                        + TextStyle.Minor + Stringify());
                }
            }

            /// <summary>
            /// Gives a warning if stack size is not at least the given size.
            /// </summary>
            /// <param name="situation">The current place in the code that requires a validation.</param>
            /// <param name="expected">The expected minimum stack size.</param>
            [Conditional("VALIDATE")]
            public void ValidateStackSizeIsAtLeast(string situation, int expected)
            {
                if (StackSize < expected)
                {
                    System.Context.BadOutput("Stack not well sized at " + TextStyle.Separate + situation + TextStyle.Base
                        + "... size = " + TextStyle.Separate + StackSize + TextStyle.Base
                        + " but should be at least " + TextStyle.Separate + expected + TextStyle.Base + " for code:\n"
                        + TextStyle.Minor + Stringify());
                }
            }

            /// <summary>
            /// Gives a warning if stack size is not at most the given size.
            /// </summary>
            /// <param name="situation">The current place in the code that requires a validation.</param>
            /// <param name="expected">The expected maxium stack size.</param>
            [Conditional("VALIDATE")]
            public void ValidateStackSizeIsAtMost(string situation, int expected)
            {
                if (StackSize > expected)
                {
                    System.Context.BadOutput("Stack not well sized at " + TextStyle.Separate + situation + TextStyle.Base
                        + "... size = " + TextStyle.Separate + StackSize + TextStyle.Base
                        + " but should be at most " + TextStyle.Separate + expected + TextStyle.Base + " for code:\n"
                        + TextStyle.Minor + Stringify());
                }
            }

            /// <summary>
            /// Creates a string of all the generated CIL code.
            /// </summary>
            /// <returns>Generated CIL code string.</returns>
            public string Stringify()
            {
#if DEBUG
                StringBuilder fullResult = new StringBuilder();
                foreach (KeyValuePair<string, object> code in Codes)
                {
                    if (code.Key == "minor")
                    {
                        fullResult.Append(TextStyle.Separate + "(Minor)" + TextStyle.Minor + ": " + code.Value + "\n");
                    }
                    else
                    {
                        fullResult.Append(TextStyle.Separate + code.Key + TextStyle.Minor + ": " + TextStyle.Base + code.Value + "\n");
                    }
                }
                return fullResult.ToString();
#else
                return "(Generator Not Tracked)";
#endif
            }

            /// <summary>
            /// When compiled in DEBUG mode, adds a code value to the <see cref="Codes"/> list.
            /// When compiled with VALIDATE set, validates the new opcode.
            /// </summary>
            /// <param name="code">The OpCode used (or 'nop' for special comments).</param>
            /// <param name="val">The value attached to the opcode, if any.</param>
            /// <param name="typeName">The special code type name, if any.</param>
            public void AddCode(OpCode code, object val, string typeName = null)
            {
#if DEBUG
                Codes.Add(new KeyValuePair<string, object>(typeName ?? code.ToString().ToLowerFast(), val));
#endif
                Validator(code, val);
            }

            /// <summary>
            /// Validation call for stack size wrangling.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="val">The object value.</param>
            /// <param name="altParams">The number of parameters if GetParameters() is not stable.</param>
            [Conditional("VALIDATE")]
            public void Validator(OpCode code, object val, int? altParams = null)
            {
                if (code == OpCodes.Nop)
                {
                    // Do nothing
                }
                if (code == OpCodes.Ret)
                {
                    ValidateStackSizeIsAtMost("return op", 1);
                    StackSize = 0;
                }
                else if (code == OpCodes.Call || code == OpCodes.Callvirt)
                {
                    if (!(val is MethodInfo method))
                    {
                        Console.WriteLine("Invalid call (code " + code + ", to object " + val + ") - not a method reference");
                    }
                    else
                    {
                        int paramCount = altParams ?? method.GetParameters().Length;
                        if (!method.IsStatic)
                        {
                            paramCount++;
                        }
                        ValidateStackSizeIsAtLeast("call opcode " + code, paramCount);
                        StackSizeChange(-paramCount);
                        if (method.ReturnType != typeof(void))
                        {
                            StackSizeChange(1);
                        }
                    }
                }
                else if (code == OpCodes.Leave || code == OpCodes.Leave_S)
                {
                    ValidateStackSizeIs("Leaving exception block", 0);
                }
                else
                {
                    switch (code.StackBehaviourPop)
                    {
                        case StackBehaviour.Pop0:
                            break;
                        case StackBehaviour.Varpop:
                        case StackBehaviour.Pop1:
                        case StackBehaviour.Popref:
                        case StackBehaviour.Popi:
                            ValidateStackSizeIsAtLeast("opcode " + code, 1);
                            StackSizeChange(-1);
                            break;
                        case StackBehaviour.Pop1_pop1:
                        case StackBehaviour.Popi_pop1:
                        case StackBehaviour.Popi_popi:
                        case StackBehaviour.Popi_popi8:
                        case StackBehaviour.Popi_popr4:
                        case StackBehaviour.Popi_popr8:
                        case StackBehaviour.Popref_pop1:
                        case StackBehaviour.Popref_popi:
                            ValidateStackSizeIsAtLeast("opcode " + code, 2);
                            StackSizeChange(-2);
                            break;
                        case StackBehaviour.Popi_popi_popi:
                        case StackBehaviour.Popref_popi_popi:
                        case StackBehaviour.Popref_popi_popi8:
                        case StackBehaviour.Popref_popi_popr4:
                        case StackBehaviour.Popref_popi_popr8:
                        case StackBehaviour.Popref_popi_popref:
                        case StackBehaviour.Popref_popi_pop1:
                            ValidateStackSizeIsAtLeast("opcode " + code, 3);
                            StackSizeChange(-3);
                            break;
                    }
                    switch (code.StackBehaviourPush)
                    {
                        case StackBehaviour.Push0:
                            break;
                        case StackBehaviour.Push1:
                        case StackBehaviour.Pushi:
                        case StackBehaviour.Pushi8:
                        case StackBehaviour.Pushr4:
                        case StackBehaviour.Pushr8:
                        case StackBehaviour.Pushref:
                        case StackBehaviour.Varpush:
                            StackSizeChange(1);
                            break;
                        case StackBehaviour.Push1_push1:
                            StackSizeChange(2);
                            break;
                    }
                }
                ValidateStackSizeIsAtLeast("post opcode " + code, 0);
            }

            /// <summary>
            /// Defines a label.
            /// </summary>
            /// <returns>The label.</returns>
            public Label DefineLabel()
            {
                return Internal.DefineLabel();
            }

            /// <summary>
            /// Starts a filtered 'try' block.
            /// Usage pattern:
            /// <code>
            /// Label exceptionLabel = BeginExceptionBlock();
            /// ... risky code ...
            /// Emit(OpCodes.Leave, exceptionLabel);
            /// BeginCatchBlock(typeof(Exception));
            /// ... catch code ...
            /// EndExceptionBlock();
            /// </code>
            /// </summary>
            /// <returns>The block label.</returns>
            public Label BeginExceptionBlock()
            {
                Label toRet = Internal.BeginExceptionBlock();
                AddCode(OpCodes.Nop, toRet, "<start try block, label>");
                ValidateStackSizeIs("Starting exception block", 0);
                return toRet;
            }

            /// <summary>
            /// Changes the stack size.
            /// </summary>
            /// <param name="amount">The amount to change by.</param>
            [Conditional("VALIDATE")]
            public void StackSizeChange(int amount)
            {
                StackSize += amount;
                AddCode(OpCodes.Nop, "<stack size move: " + amount + ", now: " + StackSize + ">", "minor");
            }

            /// <summary>
            /// Starts a catch block for a specific exception type.
            /// </summary>
            public void BeginCatchBlock(Type exType)
            {
                Internal.BeginCatchBlock(exType);
                AddCode(OpCodes.Nop, exType, "<begin catch block, type>");
                ValidateStackSizeIs("Starting catch block", 0);
                StackSizeChange(1);
            }

            /// <summary>
            /// Ends an exception block.
            /// </summary>
            public void EndExceptionBlock()
            {
                Internal.EndExceptionBlock();
                AddCode(OpCodes.Nop, null, "<EndExceptionBlock>");
                ValidateStackSizeIs("Ending exception block", 0);
            }

            /// <summary>
            /// Marks a label.
            /// </summary>
            /// <param name="label">The label.</param>
            public void MarkLabel(Label label)
            {
                Internal.MarkLabel(label);
                AddCode(OpCodes.Nop, label, "<MarkLabel>");
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            public void Emit(OpCode code)
            {
                Internal.Emit(code);
                AddCode(code, null);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="dat">The associated data.</param>
            public void Emit(OpCode code, FieldInfo dat)
            {
                Internal.Emit(code, dat);
                AddCode(code, dat);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="t">The associated data.</param>
            public void Emit(OpCode code, Type t)
            {
                Internal.Emit(code, t);
                AddCode(code, t);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="t">The associated data.</param>
            public void Emit(OpCode code, ConstructorInfo t)
            {
                Internal.Emit(code, t);
                AddCode(code, t);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="dat">The associated data.</param>
            public void Emit(OpCode code, Label[] dat)
            {
                Internal.Emit(code, dat);
                AddCode(code, dat);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="dat">The associated data.</param>
            /// <param name="altParams">The number of parameters, if GetParameters is not stable.</param>
            public void Emit(OpCode code, MethodInfo dat, int? altParams = null)
            {
                Internal.Emit(code, dat);
                AddCode(OpCodes.Nop, dat + ": " + dat.DeclaringType.Name, code.ToString().ToLowerFast());
                Validator(code, dat, altParams);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="dat">The associated data.</param>
            public void Emit(OpCode code, Label dat)
            {
                Internal.Emit(code, dat);
                AddCode(code, dat);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="dat">The associated data.</param>
            public void Emit(OpCode code, string dat)
            {
                Internal.Emit(code, dat);
                AddCode(code, dat);
            }

            /// <summary>
            /// Emits an operation.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="dat">The associated data.</param>
            public void Emit(OpCode code, int dat)
            {
                Internal.Emit(code, dat);
                AddCode(code, dat);
            }

            /// <summary>
            /// Declares a local.
            /// </summary>
            /// <param name="t">The type.</param>
            public int DeclareLocal(Type t)
            {
                int x = Internal.DeclareLocal(t).LocalIndex;
                AddCode(OpCodes.Nop, t.FullName + " as " + x, "<declare local>");
                return x;
            }

            /// <summary>
            /// Adds a comment to the developer debug of the IL output.
            /// </summary>
            /// <param name="str">The comment text.</param>
            public void Comment(string str)
            {
                AddCode(OpCodes.Nop, "-- " + str + " --", "// Comment");
            }
        }

        /// <summary>
        /// The IL code generator.
        /// </summary>
        public ILGeneratorTracker ILGen;

        /// <summary>
        /// The method being constructed.
        /// </summary>
        public MethodBuilder Method;

        /// <summary>
        /// Returns the return-type of a variable by its location ID.
        /// </summary>
        /// <param name="varId">The variable location ID.</param>
        /// <returns>The return-type of the tag.</returns>
        public TagType LocalVariableType(int varId)
        {
            for (int n = 0; n < CLVariables.Count; n++)
            {
                foreach (SingleCILVariable locVar in CLVariables[n])
                {
                    if (locVar.Index == varId)
                    {
                        return locVar.Type;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the location of a local variable, by name.
        /// Returns -1 if not found.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The location.</returns>
        public int LocalVariableLocation(string name)
        {
            return LocalVariableLocation(name, out _);
        }

        /// <summary>
        /// Returns the location of a local variable, by name.
        /// Returns -1 if not found.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type of the local variable.</param>
        /// <returns>The location.</returns>
        public int LocalVariableLocation(string name, out TagType type)
        {
            foreach (int i in LVarIDs)
            {
                foreach (SingleCILVariable locVar in CLVariables[i])
                {
                    if (locVar.Name == name)
                    {
                        type = locVar.Type;
                        return locVar.Index;
                    }
                }
            }
            type = null;
            return -1;
        }

        /// <summary>
        /// Pushes a new set of variables, to start a scope.
        /// </summary>
        public void PushVarSet()
        {
            LVarIDs.Push(CLVariables.Count);
            CLVariables.Add(new List<SingleCILVariable>());
        }

        /// <summary>
        /// Pops the newest set of variables, to end a scope.
        /// </summary>
        public void PopVarSet()
        {
            LVarIDs.Pop();
        }

        /// <summary>
        /// Creates a var-lookup dictionary for the current stack location.
        /// </summary>
        /// <returns>The var-lookup dictionary.</returns>
        public Dictionary<string, SingleCILVariable> CreateVarLookup()
        {
            Dictionary<string, SingleCILVariable> varlookup = new Dictionary<string, SingleCILVariable>(LVarIDs.Count * 3);
            foreach (int tv in LVarIDs)
            {
                foreach (SingleCILVariable tvt in CLVariables[tv])
                {
                    varlookup.Add(tvt.Name, tvt);
                }
            }
            return varlookup;
        }

        /// <summary>
        /// Adds a variable at the highest scope.
        /// </summary>
        /// <param name="var">The variable name.</param>
        /// <param name="type">The variable value.</param>
        /// <returns>The added variable's location.</returns>
        public int AddVariable(string var, TagType type)
        {
            int id = CLVarID++;
            CLVariables[LVarIDs.Peek()].Add(new SingleCILVariable(id, var, type));
            return id;
        }

        /// <summary>
        /// All known CIL Variable data sets.
        /// </summary>
        public List<List<SingleCILVariable>> CLVariables = new List<List<SingleCILVariable>>();

        /// <summary>
        /// The current stack of LVarIDs.
        /// </summary>
        public Stack<int> LVarIDs = new Stack<int>();

        /// <summary>
        /// The current CIL Var ID.
        /// </summary>
        public int CLVarID = 0;

        /// <summary>
        /// Load the entry onto the stack.
        /// </summary>
        public void LoadEntry(int entry)
        {
            ILGen.Emit(OpCodes.Ldarg_3);
            ILGen.Emit(OpCodes.Ldc_I4, entry);
            ILGen.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// Load the queue variable onto the stack.
        /// </summary>
        public void LoadQueue()
        {
            ILGen.Emit(OpCodes.Ldarg_1);
        }

        /// <summary>
        /// Loads a tag data object appropriate to the queue. Can trigger some logic to run.
        /// </summary>
        public void LoadTagData()
        {
            LoadQueue();
            ILGen.Emit(OpCodes.Call, CommandQueue.COMMANDQUEUE_GETTAGDATA);
        }
        
        /// <summary>
        /// Loads the linked command stack entry.
        /// </summary>
        public void LoadCCSE()
        {
            LoadQueue();
            ILGen.Emit(OpCodes.Ldfld, CommandQueue.COMMANDQUEUE_CURRENTENTRY);
        }

        /// <summary>
        /// Marks the command as the correct entry. Should be called with every command!
        /// </summary>
        /// <param name="entry">The entry location.</param>
        public void MarkCommand(int entry)
        {
            if (entry < Entry.Entries.Length)
            {
                ILGen.Comment("Begin command: " + entry + ") " + Entry.Entries[entry].CommandLine);
            }
            else
            {
                ILGen.Comment("End command series at: " + entry);
            }
            ILGen.Emit(OpCodes.Ldarg_2);
            ILGen.Emit(OpCodes.Ldc_I4, entry);
            ILGen.Emit(OpCodes.Stfld, IntHolder_InternalField);
        }

        /// <summary>
        /// Loads the command, the entry, and the queue, for calling an execution function.
        /// </summary>
        /// <param name="entry">The entry location.</param>
        public void PrepareExecutionCall(int entry)
        {
            MarkCommand(entry);
            LoadQueue();
            LoadEntry(entry);
        }

        /// <summary>
        /// Call the "Execute(queue, entry)" method with appropriate parameters.
        /// </summary>
        public void CallExecute(int entry, AbstractCommand cmd)
        {
            PrepareExecutionCall(entry);
            ILGen.Emit(OpCodes.Call, cmd.ExecuteMethod);
        }

        /// <summary>
        /// Makes a call to load the argument at the specified index in the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="argument">The argument index.</param>
        public void LoadArgumentObject(int entry, int argument)
        {
            Argument arg = Entry.Entries[entry].Arguments[argument];
            LoadEntry(entry);
            ILGen.Emit(OpCodes.Ldc_I4, argument);
            ILGen.Emit(OpCodes.Call, Method_GetArgumentAt);
            LoadQueue();
            ILGen.Emit(OpCodes.Ldfld, Queue_Error);
            LoadCCSE();
            ILGen.Emit(OpCodes.Call, arg.CompiledParseMethod);
        }

        /// <summary>
        /// Loads the local variable at the given index.
        /// </summary>
        /// <param name="index">The local variable index.</param>
        public void LoadLocalVariable(int index)
        {
            LoadCCSE();
            ILGen.Emit(OpCodes.Ldc_I4, index);
            ILGen.Emit(OpCodes.Call, Method_GetLocalVariableAt);
        }

        /// <summary>
        /// Emits logic that ensures the argument is of the given type, converting if needed.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="requiredType">The type it needs to be.</param>
        public void EnsureType(Argument arg, TagType requiredType)
        {
            if (ArgumentCompiler.ReturnType(arg, this) != requiredType)
            {
                LoadTagData();
                ILGen.Emit(OpCodes.Call, requiredType.CreatorMethod);
            }
        }

        /// <summary>
        /// A reference to the <see cref="GetLocalVariableAt(CompiledCommandStackEntry, int)"/> method.
        /// </summary>
        public static readonly MethodInfo Method_GetLocalVariableAt = typeof(CILAdaptationValues).GetMethod(nameof(GetLocalVariableAt));

        /// <summary>
        /// Helper method to get the local variable at the specified index.
        /// </summary>
        /// <param name="entry">The command stack entry.</param>
        /// <param name="loc">The variable location.</param>
        /// <returns>The variable's value.</returns>
        public static TemplateObject GetLocalVariableAt(CompiledCommandStackEntry entry, int loc)
        {
            return entry.LocalVariables[loc].Internal;
        }

        /// <summary>
        /// A reference to the <see cref="GetArgumentAt(CommandEntry, int)"/> method.
        /// </summary>
        public static readonly MethodInfo Method_GetArgumentAt = typeof(CILAdaptationValues).GetMethod(nameof(GetArgumentAt));

        /// <summary>
        /// Helper method to get the argument in a command entry at the specified index.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="loc">The index location.</param>
        /// <returns>The argument.</returns>
        public static Argument GetArgumentAt(CommandEntry entry, int loc)
        {
            return entry.Arguments[loc];
        }
    }
}
