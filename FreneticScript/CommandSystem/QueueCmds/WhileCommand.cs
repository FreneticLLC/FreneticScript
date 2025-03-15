//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.UtilitySystems;

namespace FreneticScript.CommandSystem.QueueCmds;

class WhileCommandData : AbstractCommandEntryData
{
    public Argument[] ComparisonArgs;
    public int Index;
}

/// <summary>The While command.</summary>
public class WhileCommand : AbstractCommand
{
    // TODO: Meta!

    // TODO: Compile!

    /// <summary>Constructs the while command.</summary>
    public WhileCommand()
    {
        Name = "while";
        Arguments = "'stop'/'next'/<comparisons>";
        Description = "Executes the following block of commands continuously until the argument is false.";
        IsFlow = true;
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = -1;
        IsBreakable = true;
    }

    /// <summary>Represents the <see cref="TryWhileCIL(CommandQueue, CommandEntry, IntegerTag)"/> method.</summary>
    public static MethodInfo TryWhileCILMethod = typeof(WhileCommand).GetMethod(nameof(TryWhileCIL));

    /// <summary>Represents the <see cref="TryWhileCILNoDebug(CommandQueue, int, IntegerTag)"/> method.</summary>
    public static MethodInfo TryWhileCILMethodNoDebug = typeof(WhileCommand).GetMethod(nameof(TryWhileCILNoDebug));

    /// <summary>Represents the <see cref="TryWhileNumberedCIL(CommandQueue, CommandEntry)"/> method.</summary>
    public static MethodInfo TryWhileNumberedCILMethod = typeof(WhileCommand).GetMethod(nameof(TryWhileNumberedCIL));

    /// <summary>Represents the <see cref="TryWhileNumberedCIL_NoDebug(CommandQueue, CommandEntry)"/> method.</summary>
    public static MethodInfo TryWhileNumberedCIL_NoDebugMethod = typeof(WhileCommand).GetMethod(nameof(TryWhileNumberedCIL_NoDebug));

    /// <summary>Represents the <see cref="DebugStop(CommandQueue, CommandEntry)"/> method.</summary>
    public static MethodInfo DebugStopMethod = typeof(WhileCommand).GetMethod(nameof(DebugStop));

    /// <summary>Represents the <see cref="DebugNext(CommandQueue, CommandEntry)"/> method.</summary>
    public static MethodInfo DebugNextMethod = typeof(WhileCommand).GetMethod(nameof(DebugNext));

    /// <summary>Represents the <see cref="CreateIndexObject"/> method.</summary>
    public static MethodInfo CreateIndexObjectMethod = typeof(WhileCommand).GetMethod(nameof(CreateIndexObject));

    /// <summary>Adapts a command entry to CIL.</summary>
    /// <param name="values">The adaptation-relevant values.</param>
    /// <param name="entry">The present entry ID.</param>
    public override void AdaptToCIL(CILAdaptationValues values, int entry)
    {
        CommandEntry cent = values.CommandAt(entry);
        bool db = cent.DBMode <= DebugMode.FULL;
        if (cent.IsCallback)
        {
            string sn = values.Entry.Entries[cent.BlockStart - 1].GetSaveNameNoParse("while_index");
            int lvar_ind_loc = cent.VarLoc(sn);
            values.LoadQueue();
            if (db)
            {
                values.LoadEntry(entry);
            }
            else
            {
                values.ILGen.Emit(OpCodes.Ldc_I4, cent.BlockStart - 1);
            }
            values.LoadLocalVariable(lvar_ind_loc);
            values.ILGen.Emit(OpCodes.Call, db ? TryWhileCILMethod : TryWhileCILMethodNoDebug);
            values.ILGen.Emit(OpCodes.Brtrue, values.Entry.AdaptedILPoints[cent.BlockStart]);
            return;
        }
        string arg = cent.Arguments[0].ToString();
        if (arg == "stop")
        {
            for (int i = entry - 1; i >= 0; i--)
            {
                CommandEntry nextEntry = values.Entry.Entries[i];
                if (nextEntry.Command is not WhileCommand || nextEntry.IsCallback)
                {
                    continue;
                }
                string a0 = nextEntry.Arguments[0].ToString();
                if (a0 != "stop" && a0 != "next" && nextEntry.InnerCommandBlock != null)
                {
                    if (db)
                    {
                        values.LoadQueue();
                        values.LoadEntry(entry);
                        values.ILGen.Emit(OpCodes.Call, DebugStopMethod);
                    }
                    values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[nextEntry.BlockEnd + 2]);
                    return;
                }
            }
            throw new ErrorInducedException("Invalid 'while stop' command: not inside a while block!");
        }
        else if (arg == "next")
        {
            for (int i = entry - 1; i >= 0; i--)
            {
                CommandEntry nextEntry = values.Entry.Entries[i];
                if (nextEntry.Command is not WhileCommand || nextEntry.IsCallback)
                {
                    continue;
                }
                string a0 = nextEntry.ToString();
                if (a0 != "stop" && a0 != "next" && nextEntry.InnerCommandBlock != null)
                {
                    if (db)
                    {
                        values.LoadQueue();
                        values.LoadEntry(entry);
                        values.ILGen.Emit(OpCodes.Call, DebugNextMethod);
                    }
                    values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[nextEntry.BlockEnd + 1]);
                    return;
                }
            }
            throw new ErrorInducedException("Invalid 'while next' command: not inside a while block!");
        }
        else
        {
            values.LoadQueue();
            values.LoadEntry(entry);
            values.ILGen.Emit(OpCodes.Call, CreateIndexObjectMethod);
            values.ILGen.Emit(OpCodes.Stfld, cent.VarLookup[cent.GetSaveNameNoParse("while_index")].Field);
            values.ILGen.Emit(OpCodes.Call, db ? TryWhileNumberedCILMethod : TryWhileNumberedCIL_NoDebugMethod);
            values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
        }
    }

    /// <summary>Prepares to adapt a command entry to CIL.</summary>
    /// <param name="values">The adaptation-relevant values.</param>
    /// <param name="entry">The present entry ID.</param>
    public override void PreAdaptToCIL(CILAdaptationValues values, int entry)
    {
        CommandEntry cent = values.Entry.Entries[entry];
        if (cent.IsCallback)
        {
            values.PopVarSet();
            return;
        }
        string arg = cent.Arguments[0].ToString();
        if (arg == "next" || arg == "stop")
        {
            return;
        }
        values.PushVarSet();
        string sn = cent.GetSaveNameNoParse("while_index");
        if (values.LocalVariableLocation(sn) >= 0)
        {
            throw new ErrorInducedException("Already have a while_index var (labeled '" + sn + "')?!");
        }
        TagType type = cent.TagSystem.Types.Type_Integer;
        values.AddVariable(sn, type);
    }

    /// <summary>Shows debug for a while 'stop' command.</summary>
    /// <param name="queue">The command queue.</param>
    /// <param name="entry">The command entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugStop(CommandQueue queue, CommandEntry entry)
    {
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "While loop stopped successfully.");
        }
    }

    /// <summary>Shows debug for a while 'next' command.</summary>
    /// <param name="queue">The command queue.</param>
    /// <param name="entry">The command entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugNext(CommandQueue queue, CommandEntry entry)
    {
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "While loop jumping to next iteration.");
        }
    }

    /// <summary>Executes the callback part of the while command, without debug output.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry_ind">Entry to be executed.</param>
    /// <param name="integer">While Index holder.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWhileCILNoDebug(CommandQueue queue, int entry_ind, IntegerTag integer)
    {
        WhileCommandData dat = queue.CurrentRunnable.EntryData[entry_ind] as WhileCommandData;
        integer.Internal = ++dat.Index;
        return IfCommand.TryIf(queue, null, [.. dat.ComparisonArgs]);
    }

    /// <summary>Executes the callback part of the while command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    /// <param name="integer">While Index holder.</param>
    public static bool TryWhileCIL(CommandQueue queue, CommandEntry entry, IntegerTag integer)
    {
        WhileCommandData dat = queue.CurrentRunnable.EntryData[entry.BlockStart - 1] as WhileCommandData;
        integer.Internal = ++dat.Index;
        if (IfCommand.TryIf(queue, entry, [.. dat.ComparisonArgs]))
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "While looping: " + TextStyle.Separate + dat.Index + TextStyle.Base + "...");
            }
            return true;
        }
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "While stopping.");
        }
        return false;
    }

    /// <summary>Executes the numbered input part of the while command, without debug.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static bool TryWhileNumberedCIL_NoDebug(CommandQueue queue, CommandEntry entry)
    {
        bool success = IfCommand.TryIf(queue, entry, [.. entry.Arguments]);
        if (!success)
        {
            return false;
        }
        entry.SetData(queue, new WhileCommandData() { Index = 1, ComparisonArgs = entry.Arguments });
        return true;
    }

    /// <summary>Executes the comparison input part of the while command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static bool TryWhileNumberedCIL(CommandQueue queue, CommandEntry entry)
    {
        bool success = IfCommand.TryIf(queue, entry, [.. entry.Arguments]);
        if (!success)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Not looping.");
            }
            return false;
        }
        entry.SetData(queue, new WhileCommandData() { Index = 1, ComparisonArgs = entry.Arguments });
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "While looping...");
        }
        return true;
    }

    /// <summary>Creates a while index object.</summary>
    /// <returns>The index object.</returns>
    public static IntegerTag CreateIndexObject()
    {
        return new IntegerTag(1);
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        queue.HandleError(entry, "Cannot Execute() a while, must compile!");
    }
}
