//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

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
        /// Represents the <see cref="CompiledCommandStackEntry.Entries"/> field.
        /// </summary>
        public static readonly FieldInfo EntriesField = typeof(CompiledCommandStackEntry).GetField(nameof(CompiledCommandStackEntry.Entries));

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
        /// Loads the linked command runnable.
        /// </summary>
        public void LoadRunnable()
        {
            ILGen.Emit(OpCodes.Ldarg_0);
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
            LoadRunnable();
            ILGen.Emit(OpCodes.Call, arg.CompiledParseMethod);
        }

        /// <summary>
        /// Loads the local variable at the given index.
        /// </summary>
        /// <param name="index">The local variable index.</param>
        public void LoadLocalVariable(int index)
        {
            LoadRunnable();
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
        /// A reference to the <see cref="GetLocalVariableAt(CompiledCommandRunnable, int)"/> method.
        /// </summary>
        public static readonly MethodInfo Method_GetLocalVariableAt = typeof(CILAdaptationValues).GetMethod(nameof(GetLocalVariableAt));

        /// <summary>
        /// Helper method to get the local variable at the specified index.
        /// </summary>
        /// <param name="runnable">The command runnable.</param>
        /// <param name="loc">The variable location.</param>
        /// <returns>The variable's value.</returns>
        public static TemplateObject GetLocalVariableAt(CompiledCommandRunnable runnable, int loc)
        {
            return runnable.LocalVariables[loc].Internal;
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
