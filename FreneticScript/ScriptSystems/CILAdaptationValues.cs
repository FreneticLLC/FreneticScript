//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;

namespace FreneticScript.ScriptSystems;

/// <summary>Represents one CIL adapter variable.</summary>
/// <param name="_index">The variable index.</param>
/// <param name="_name">The variable name.</param>
/// <param name="_type">The variable type.</param>
/// <param name="_field">The field for this variable.</param>
public class SingleCILVariable(int _index, string _name, TagReturnType _type, FieldInfo _field)
{
    /// <summary>The index of the local variable.</summary>
    public int Index = _index;

    /// <summary>The name of the variable.</summary>
    public string Name = _name;

    /// <summary>The type of the variable.</summary>
    public TagReturnType Type = _type;

    /// <summary>The field that holds this variable's value.</summary>
    public FieldInfo Field = _field;
}

/// <summary>Holds all data needed for CIL adaptation.</summary>
public class CILAdaptationValues
{
    /// <summary>The compiled CSE involved.</summary>
    public CompiledCommandStackEntry Entry;

    /// <summary>The compiling script.</summary>
    public CommandScript Script;

    /// <summary>The debug mode currently in use.</summary>
    public DebugMode DBMode;

    /// <summary>Gets the command at the index.</summary>
    /// <param name="index">The index value.</param>
    /// <returns>The command.</returns>
    public CommandEntry CommandAt(int index)
    {
        return Entry.Entries[index];
    }

    /// <summary>Represents the <see cref="CommandEntry.Command"/> field.</summary>
    public static readonly FieldInfo Entry_CommandField = typeof(CommandEntry).GetField(nameof(CommandEntry.Command));

    /// <summary>Represents the <see cref="CommandEntry.Arguments"/> field.</summary>
    public static readonly FieldInfo Entry_ArgumentsField = typeof(CommandEntry).GetField(nameof(CommandEntry.Arguments));

    /// <summary>Represents the <see cref="CommandQueue.Error"/> field.</summary>
    public static readonly FieldInfo Queue_Error = typeof(CommandQueue).GetField(nameof(CommandQueue.Error));

    /// <summary>The type of the class <see cref="CommandEntry"/> class.</summary>
    public static readonly Type CommandEntryType = typeof(CommandEntry);

    /// <summary>The IL code generator.</summary>
    public ILGeneratorTracker ILGen;

    /// <summary>The method being constructed.</summary>
    public MethodBuilder Method;

    /// <summary>The type being constructed.</summary>
    public TypeBuilder Type;

    /// <summary>The trackers list (if tracking CIL output).</summary>
    public List<ILGeneratorTracker> Trackers;

    /// <summary>Returns the data of a variable by its location ID.</summary>
    /// <param name="varId">The variable location ID.</param>
    /// <returns>The variable data.</returns>
    public SingleCILVariable LocalVariableData(int varId)
    {
        if (varId < 0 || varId >= Variables.Count)
        {
            return null;
        }
        return Variables[varId];
    }

    /// <summary>Returns the return-type of a variable by its location ID.</summary>
    /// <param name="varId">The variable location ID.</param>
    /// <returns>The return-type of the tag.</returns>
    public TagReturnType LocalVariableType(int varId)
    {
        return LocalVariableData(varId)?.Type ?? default;
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
    public int LocalVariableLocation(string name, out TagReturnType type)
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
        type = default;
        return -1;
    }

    /// <summary>Pushes a new set of variables, to start a scope.</summary>
    public void PushVarSet()
    {
        LVarIDs.Push(CLVariables.Count);
        CLVariables.Add([]);
    }

    /// <summary>Pops the newest set of variables, to end a scope.</summary>
    public void PopVarSet()
    {
        LVarIDs.Pop();
    }

    /// <summary>Creates a var-lookup dictionary for the current stack location.</summary>
    /// <returns>The var-lookup dictionary.</returns>
    public Dictionary<string, SingleCILVariable> CreateVarLookup()
    {
        Dictionary<string, SingleCILVariable> varlookup = new(LVarIDs.Count * 3);
        foreach (int tv in LVarIDs)
        {
            foreach (SingleCILVariable tvt in CLVariables[tv])
            {
                varlookup.Add(tvt.Name, tvt);
            }
        }
        return varlookup;
    }

    /// <summary>Adds a variable at the highest scope.</summary>
    /// <param name="var">The variable name.</param>
    /// <param name="type">The variable value.</param>
    /// <returns>The added variable's location.</returns>
    public int AddVariable(string var, TagType type)
    {
        int id = CLVarID++;
        Type rawType;
        if (type.Meta.RawInternal)
        {
            rawType = type.RawInternalType;
        }
        else
        {
            rawType = type.RawType;
        }
        FieldInfo newField = Type.DefineField("_field_locVar_" + id, rawType, FieldAttributes.Public);
        SingleCILVariable variable = new(id, var, new TagReturnType(type, type.Meta.RawInternal), newField);
        CLVariables[LVarIDs.Peek()].Add(variable);
        Variables.Add(variable);
        return id;
    }

    /// <summary>A list of all variables.</summary>
    public List<SingleCILVariable> Variables = [];

    /// <summary>All known CIL Variable data sets.</summary>
    public List<List<SingleCILVariable>> CLVariables = [];

    /// <summary>The current stack of LVarIDs.</summary>
    public Stack<int> LVarIDs = new();

    /// <summary>Fields for arguments, if generated.</summary>
    public FieldInfo[][] ArgumentFields;

    /// <summary>The current CIL Var ID.</summary>
    public int CLVarID = 0;

    /// <summary>Fields storing each command entry.</summary>
    public FieldInfo[] EntryFields;

    /// <summary>Load the entry onto the stack.</summary>
    public void LoadEntry(int entry)
    {
        LoadRunnable();
        ILGen.Emit(OpCodes.Ldfld, EntryFields[entry]);
    }

    /// <summary>Load the queue variable onto the stack.</summary>
    public void LoadQueue()
    {
        ILGen.Emit(OpCodes.Ldarg_1);
    }

    /// <summary>Loads a tag data object appropriate to the queue. Can trigger some logic to run.</summary>
    public void LoadTagData()
    {
        LoadQueue();
        ILGen.Emit(OpCodes.Call, CommandQueue.COMMANDQUEUE_GETTAGDATA);
    }

    /// <summary>Loads the linked command runnable.</summary>
    public void LoadRunnable()
    {
        ILGen.Emit(OpCodes.Ldarg_0);
    }

    /// <summary>Marks the command as the correct entry. Should be called with every command!</summary>
    /// <param name="entry">The entry location.</param>
    public void MarkCommand(int entry)
    {
        if (entry < Entry.Entries.Length)
        {
            ILGen.Comment("Begin standard command: " + entry + ") " + Entry.Entries[entry].CommandLine);
        }
        else
        {
            ILGen.Comment("End command series at: " + entry);
        }
        ILGen.Emit(OpCodes.Ldarg_0);
        ILGen.Emit(OpCodes.Ldc_I4, entry);
        ILGen.Emit(OpCodes.Stfld, CompiledCommandRunnable.IndexField);
    }

    /// <summary>Loads the command, the entry, and the queue, for calling an execution function.</summary>
    /// <param name="entry">The entry location.</param>
    public void PrepareExecutionCall(int entry)
    {
        MarkCommand(entry);
        LoadQueue();
        LoadEntry(entry);
    }

    /// <summary>Call the "Execute(queue, entry)" method with appropriate parameters.</summary>
    public void CallExecute(int entry, AbstractCommand cmd)
    {
        PrepareExecutionCall(entry);
        ILGen.Emit(OpCodes.Call, cmd.ExecuteMethod);
    }

    /// <summary>Makes a call to load the argument at the specified index in the specified entry.</summary>
    /// <param name="entry">The entry.</param>
    /// <param name="argument">The argument index.</param>
    public void LoadArgumentObject(int entry, int argument)
    {
        CommandEntry curEnt = Entry.Entries[entry];
        Argument arg = curEnt.Arguments[argument];
        if (ArgumentFields[entry] == null)
        {
            ArgumentFields[entry] = new FieldInfo[curEnt.Arguments.Length];
        }
        if (ArgumentFields[entry][argument] == null)
        {
            ArgumentFields[entry][argument] = Type.DefineField("_field_entry_" + entry + "_argument_" + argument, typeof(Argument), FieldAttributes.Public | FieldAttributes.InitOnly);
        }
        LoadRunnable();
        ILGen.Emit(OpCodes.Ldfld, ArgumentFields[entry][argument]);
        LoadQueue();
        ILGen.Emit(OpCodes.Ldfld, Queue_Error);
        LoadRunnable();
        ILGen.Emit(OpCodes.Call, arg.RawParseMethod ?? arg.CompiledParseMethod, 2);
    }

    /// <summary>Loads the local variable at the given index.</summary>
    /// <param name="index">The local variable index.</param>
    public void LoadLocalVariable(int index)
    {
        LoadRunnable();
        ILGen.Emit(OpCodes.Ldfld, LocalVariableData(index).Field);
    }

    /// <summary>Emits logic that ensures the argument is of the given type, converting if needed.</summary>
    /// <param name="currentType">The current object type.</param>
    /// <param name="requiredType">The type it needs to be.</param>
    public void EnsureType(TagReturnType currentType, TagReturnType requiredType)
    {
        if (currentType.Type != requiredType.Type)
        {
            if (currentType.IsRaw)
            {
                ILGen.Emit(OpCodes.Newobj, currentType.Type.RawInternalConstructor);
            }
            LoadTagData();
            ILGen.Emit(OpCodes.Call, requiredType.Type.CreatorMethod);
            if (requiredType.IsRaw)
            {
                ILGen.Emit(OpCodes.Ldfld, requiredType.Type.RawInternalField);
            }
        }
        else if (currentType.IsRaw && !requiredType.IsRaw)
        {
            ILGen.Emit(OpCodes.Newobj, currentType.Type.RawInternalConstructor);
        }
        else if (!currentType.IsRaw && requiredType.IsRaw)
        {
            ILGen.Emit(OpCodes.Ldfld, currentType.Type.RawInternalField);
        }
    }

    /// <summary>Emits logic that ensures the argument is of the given type, converting if needed.</summary>
    /// <param name="arg">The argument.</param>
    /// <param name="requiredType">The type it needs to be.</param>
    public void EnsureType(Argument arg, TagReturnType requiredType)
    {
        EnsureType(ArgumentCompiler.ReturnType(arg, this), requiredType);
    }
}
