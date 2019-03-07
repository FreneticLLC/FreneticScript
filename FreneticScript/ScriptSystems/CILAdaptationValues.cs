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

namespace FreneticScript.ScriptSystems
{
    /// <summary>
    /// Holder of CIL Variable data.
    /// </summary>
    public class CILVariables
    {
        /// <summary>
        /// A map of local variables to track.
        /// </summary>
        public List<Tuple<int, string, TagType>> LVariables = new List<Tuple<int, string, TagType>>();
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
        /// Represents the <see cref="IntHolder.Internal"/> field.
        /// </summary>
        public static readonly FieldInfo IntHolder_InternalField = typeof(IntHolder).GetField(nameof(IntHolder.Internal));

        /// <summary>
        /// Represents the <see cref="CommandQueue.SetLocalVar(int, TemplateObject)"/> method.
        /// </summary>
        public static readonly MethodInfo Queue_SetLocalVarMethod = typeof(CommandQueue).GetMethod(nameof(CommandQueue.SetLocalVar), new Type[] { typeof(int), typeof(TemplateObject) });

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
            /// All codes generated. Only has a value when compiled in DEBUG mode.
            /// </summary>
            public List<KeyValuePair<OpCode, object>> Codes
#if DEBUG
                = new List<KeyValuePair<OpCode, object>>()
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
                    Console.WriteLine("Stack not well sized at " + situation + "... size = " + StackSize + " but should be " + expected + " for code:\n" + Stringify());
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
                    Console.WriteLine("Stack not well sized at " + situation + "... size = " + StackSize + " but should be at least " + expected + " for code:\n" + Stringify());
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
                    Console.WriteLine("Stack not well sized at " + situation + "... size = " + StackSize + " but should be at most " + expected + " for code:\n" + Stringify());
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
                foreach (KeyValuePair<OpCode, object> code in Codes)
                {
                    fullResult.Append(code.Key.Name + ": " + code.Value + "\n");
                }
                return fullResult.ToString();
#else
                return "(Generator Not Tracked)";
#endif
            }

            /// <summary>
            /// When compiled in DEBUG mode, adds a code value to the <see cref="Codes"/> list. Does nothing outside of DEBUG mode.
            /// </summary>
            /// <param name="code">The OpCode used (or 'nop' for special comments).</param>
            /// <param name="val">The value attached to the opcode, if any.</param>
            [Conditional("DEBUG")]
            public void AddCode(OpCode code, object val)
            {
                Codes.Add(new KeyValuePair<OpCode, object>(code, val));
                Validator(code, val);
            }

            /// <summary>
            /// Validation call for stack size wrangling.
            /// </summary>
            /// <param name="code">The operation code.</param>
            /// <param name="val">The object value.</param>
            [Conditional("VALIDATE")]
            public void Validator(OpCode code, object val)
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
                        int paramCount = method.GetParameters().Length;
                        if (!method.IsStatic)
                        {
                            paramCount++;
                        }
                        ValidateStackSizeIsAtLeast("calll opcode " + code, paramCount);
                        StackSize -= paramCount;
                        if (method.ReturnType != typeof(void))
                        {
                            StackSize += 1;
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
                            StackSize -= 1;
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
                            StackSize -= 2;
                            break;
                        case StackBehaviour.Popi_popi_popi:
                        case StackBehaviour.Popref_popi_popi:
                        case StackBehaviour.Popref_popi_popi8:
                        case StackBehaviour.Popref_popi_popr4:
                        case StackBehaviour.Popref_popi_popr8:
                        case StackBehaviour.Popref_popi_popref:
                        case StackBehaviour.Popref_popi_pop1:
                            ValidateStackSizeIsAtLeast("opcode " + code, 3);
                            StackSize -= 3;
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
                            StackSize += 1;
                            break;
                        case StackBehaviour.Push1_push1:
                            StackSize += 2;
                            break;
                    }
                }
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
                AddCode(OpCodes.Nop, "<start try block, label>: " + toRet);
#if VALIDATE
                ValidateStackSizeIs("Starting exception block", 0);
#endif
                return toRet;
            }

            /// <summary>
            /// Starts a catch block for a specific exception type.
            /// </summary>
            public void BeginCatchBlock(Type exType)
            {
                Internal.BeginCatchBlock(exType);
                AddCode(OpCodes.Nop, "<begin catch block, type:> " + exType.FullName);
#if VALIDATE
                ValidateStackSizeIs("Starting catch block", 0);
                StackSize += 1;
#endif
            }

            /// <summary>
            /// Ends an exception block.
            /// </summary>
            public void EndExceptionBlock()
            {
                Internal.EndExceptionBlock();
                AddCode(OpCodes.Nop, "<end exception block>");
#if VALIDATE
                ValidateStackSizeIs("Ending exception block", 0);
#endif
            }

            /// <summary>
            /// Marks a label.
            /// </summary>
            /// <param name="label">The label.</param>
            public void MarkLabel(Label label)
            {
                Internal.MarkLabel(label);
                AddCode(OpCodes.Nop, "<Mark label>: " + label);
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
            public void Emit(OpCode code, MethodInfo dat)
            {
                Internal.Emit(code, dat);
                AddCode(OpCodes.Nop, code + ": " + dat + ": " + dat.DeclaringType.Name);
                Validator(code, dat);
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
                AddCode(OpCodes.Nop, "<Declare local>: " + t.FullName + " as " + x);
                return x;
            }

            /// <summary>
            /// Adds a comment to the developer debug of the IL output.
            /// </summary>
            /// <param name="str">The comment text.</param>
            public void Comment(string str)
            {
                AddCode(OpCodes.Nop, "// Comment -- " + str + " --");
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
                for (int i = 0; i < CLVariables[n].LVariables.Count; i++)
                {
                    if (CLVariables[n].LVariables[i].Item1 == varId)
                    {
                        return CLVariables[n].LVariables[i].Item3;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the location of a local variable's name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The location.</returns>
        public int LocalVariableLocation(string name)
        {
            return LocalVariableLocation(name, out _);
        }

        /// <summary>
        /// Returns the location of a local variable's name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type of the local variable.</param>
        /// <returns>The location.</returns>
        public int LocalVariableLocation(string name, out TagType type)
        {
            foreach (int i in LVarIDs)
            {
                for (int x = 0; x < CLVariables[i].LVariables.Count; x++)
                {
                    if (CLVariables[i].LVariables[x].Item2 == name)
                    {
                        type = CLVariables[i].LVariables[x].Item3;
                        return CLVariables[i].LVariables[x].Item1;
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
            CLVariables.Add(new CILVariables());
        }

        /// <summary>
        /// Pops the newest set of variables, to end a scope.
        /// </summary>
        public void PopVarSet()
        {
            LVarIDs.Pop();
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
            CLVariables[LVarIDs.Peek()].LVariables.Add(new Tuple<int, string, TagType>(id, var, type));
            return id;
        }

        /// <summary>
        /// All known CIL Variable data sets.
        /// </summary>
        public List<CILVariables> CLVariables = new List<CILVariables>();

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
    }
}
