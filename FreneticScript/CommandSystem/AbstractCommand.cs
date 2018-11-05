//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#if DEBUG
#define VALIDATE
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.Arguments;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using FreneticScript.ScriptSystems;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// The base for a command.
    /// </summary>
    public abstract class AbstractCommand
    {
        /// <summary>
        /// Represents the "Execute(CommandQueue, CommandEntry)" method for this command.
        /// </summary>
        public MethodInfo ExecuteMethod = null;

        /// <summary>
        /// Initializes the abstract command.
        /// </summary>
        public AbstractCommand()
        {
            ExecuteMethod = GetType().GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);
        }

        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name = "NAME:UNSET";

        /// <summary>
        /// The system that owns this command.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// A short explanation of the arguments of the command.
        /// </summary>
        public string Arguments = "ARGUMENTS:UNSET";

        /// <summary>
        /// A short explanation of what the command does.
        /// </summary>
        public string Description = "DESCRIPTION:UNSET";

        /// <summary>
        /// In what way the command saves. Also set <see cref="DefaultSaveName"/> if relevant.
        /// </summary>
        public CommandSaveMode SaveMode = CommandSaveMode.NO_SAVE;

        /// <summary>
        /// The default save name, if <see cref="SaveMode"/> is set to <see cref="CommandSaveMode.DEFAULT_NAME"/>.
        /// </summary>
        public string DefaultSaveName = null;

        /// <summary>
        /// Whether the command is for debugging purposes.
        /// </summary>
        public bool IsDebug = false;

        /// <summary>
        /// Whether the 'break' command can be used on this command.
        /// </summary>
        public bool IsBreakable = false;

        /// <summary>
        /// Whether the command is part of a script's flow rather than for normal client use.
        /// </summary>
        public bool IsFlow = false;

        /// <summary>
        /// Whether the command can be &amp;waited on.
        /// </summary>
        public bool Waitable = false;

        /// <summary>
        /// Whether the command can be run off the primary tick.
        /// NOTE: These mostly have yet to be confirmed! They are purely theoretical!
        /// </summary>
        public bool Asyncable = false;

        /// <summary>
        /// How many arguments the command can have minimum.
        /// </summary>
        public int MinimumArguments = 0;

        /// <summary>
        /// How many arguments the command can have maximum.
        /// </summary>
        public int MaximumArguments = 100;

        /// <summary>
        /// The expected object type getters for a command.
        /// </summary>
        public List<Func<TemplateObject, TemplateObject>> ObjectTypes = null;
        
        /// <summary>
        /// Tests if the CommandEntry is valid for this command at pre-process time.
        /// </summary>
        /// <param name="entry">The entry to test</param>
        /// <returns>An error message (with tags), or null for none.</returns>
        public virtual string TestForValidity(CommandEntry entry)
        {
            if (entry.Arguments.Count < MinimumArguments)
            {
                return "Not enough arguments. Expected at least: " + MinimumArguments + ". Usage: " + TagParser.Escape(Arguments) + ", found only: " + TagParser.Escape(entry.AllOriginalArguments());
            }
            if (MaximumArguments != -1 && entry.Arguments.Count > MaximumArguments)
            {
                return "Too many arguments. Expected no more than: " + MaximumArguments + ". Usage: " + TagParser.Escape(Arguments) + ", found: " + TagParser.Escape(entry.AllOriginalArguments());
            }
            if (ObjectTypes != null)
            {
                for (int i = 0; i < entry.Arguments.Count; i++)
                {
                    if (entry.Arguments[i].Bits.Length == 1
                        && entry.Arguments[i].Bits[0] is TextArgumentBit
                        && i < ObjectTypes.Count)
                    {
                        TemplateObject obj = ObjectTypes[i].Invoke(((TextArgumentBit)entry.Arguments[i].Bits[0]).InputValue);
                        if (obj == null)
                        {
                            return "Invalid argument '" + TagParser.Escape(entry.Arguments[i].ToString())
                                + "', translates to internal NULL for this command's input expectation (Command is " + TagParser.Escape(entry.Command.Name) + "). (Dev note: expectation is " + ObjectTypes[i].Method.Name + ")";
                        }
                        ((TextArgumentBit)entry.Arguments[i].Bits[0]).InputValue = obj;
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets the follower (callback) entry for an entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public CommandEntry GetFollower(CommandEntry entry)
        {
            return new CommandEntry(entry.Name + " \0CALLBACK", entry.BlockStart, entry.BlockEnd, entry.Command, new List<Argument>() { new Argument() { Bits = new ArgumentBit[] {
                new TextArgumentBit("\0CALLBACK", false, true) } } }, entry.Name, 0, entry.ScriptName, entry.ScriptLine, entry.FairTabulation + "    ", entry.System);
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        public virtual void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            values.CallExecute(entry, this);
        }

        /// <summary>
        /// Prepares to adapt a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        public virtual void PreAdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            if (SaveMode == CommandSaveMode.DEFAULT_NAME && DefaultSaveName != null)
            {
                PreAdaptSaveMode(values, entry, true, cent.System.TagSystem.Type_Dynamic, DefaultSaveName);
            }
            if (SaveMode != CommandSaveMode.NO_SAVE)
            {
                string saveName = cent.GetSaveNameNoParse(DefaultSaveName);
                if (saveName != null)
                {
                    int varLoc = values.LocalVariableLocation(saveName, out TagType preVarType);
                    cent.SaveLoc = varLoc;
                }
            }
        }

        /// <summary>
        /// Pre-Adapt helper for save targets.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        /// <param name="canPreExist">Whether the variable is allowed to already exist.</param>
        /// <param name="tagType">The required type.</param>
        /// <param name="defaultName">The default name (or null if none).</param>
        public void PreAdaptSaveMode(CILAdaptationValues values, int entry, bool canPreExist, TagType tagType, string defaultName = null)
        {
            CommandEntry cent = values.CommandAt(entry);
            string saveName = cent.GetSaveNameNoParse(defaultName);
            if (saveName == null)
            {
                throw new ErrorInducedException("Command requires a save name, but none was given.");
            }
            int preVarLoc = values.LocalVariableLocation(saveName, out TagType preVarType);
            if (preVarLoc >= 0)
            {
                if (!canPreExist)
                {
                    throw new ErrorInducedException("Already have a save target var (labeled '" + saveName + "')?!");
                }
                if (preVarType != tagType)
                {
                    throw new ErrorInducedException("Already have a save target var (labeled '" + saveName + "', with type '" + preVarType.TypeName + "') of wrong type (expected '" + tagType.TypeName + "').");
                }
            }
            else
            {
                values.AddVariable(saveName, tagType);
            }
        }

        /// <summary>
        /// Gets the save variable location (used in Adapt, after using <see cref="PreAdaptSaveMode(CILAdaptationValues, int, bool, TagType, string)"/> in PreAdapt).
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        /// <param name="defaultName">The default name (or null if none).</param>
        public int GetSaveLoc(CILAdaptationValues values, int entry, string defaultName = null)
        {
            CommandEntry cent = values.CommandAt(entry);
            string sn = values.Entry.Entries[cent.BlockStart - 1].GetSaveNameNoParse(defaultName);
            return cent.VarLoc(sn);
        }

        /// <summary>
        /// Adjust list of commands that are formed by an inner block.
        /// </summary>
        /// <param name="entry">The producing entry.</param>
        /// <param name="input">The block of commands.</param>
        /// <param name="fblock">The final block to add to the entry.</param>
        public virtual void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            input.Add(GetFollower(entry));
        }
        
        /// <summary>
        /// Displays the usage information on a command to the console.
        /// </summary>
        /// <param name="queue">The associated queue.</param>
        /// <param name="entry">The CommandEntry data to show usage help to.</param>
        /// <param name="doError">Whether to end with an error.</param>
        /// <param name="cmd">The command to show help for - if unspecified, will get from the entry.</param>
        public static void ShowUsage(CommandQueue queue, CommandEntry entry, bool doError = true, AbstractCommand cmd = null)
        {
            if (cmd == null)
            {
                cmd = entry.Command;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.InfoOutput(queue, TextStyle.Color_Separate + cmd.Name + TextStyle.Color_Base + ": " + cmd.Description);
                entry.InfoOutput(queue, TextStyle.Color_Commandhelp + "Usage: /" + cmd.Name + " " + cmd.Arguments);
                if (cmd.IsDebug)
                {
                    entry.InfoOutput(queue, "Note: This command is intended for debugging purposes.");
                }
            }
            if (doError)
            {
                queue.HandleError(entry, "Invalid arguments or not enough arguments!");
            }
        }
    }

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
        /// Represents the <see cref="CompiledCommandRunnable.CSEntry"/> field.
        /// </summary>
        public static readonly FieldInfo EntryField = typeof(CompiledCommandRunnable).GetField(nameof(CompiledCommandRunnable.CSEntry));

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
        public void AddVariable(string var, TagType type)
        {
            CLVariables[LVarIDs.Peek()].LVariables.Add(new Tuple<int, string, TagType>(CLVarID++, var, type));
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

    /// <summary>
    /// Enumeration of modes describing the way a command saves.
    /// </summary>
    public enum CommandSaveMode
    {
        /// <summary>
        /// The command does not save.
        /// </summary>
        NO_SAVE = 0,
        /// <summary>
        /// The command can only save when a name is given.
        /// </summary>
        REQUIRED_NAME = 1,
        /// <summary>
        /// The command has a default save name value.
        /// </summary>
        DEFAULT_NAME = 2
    }
}
