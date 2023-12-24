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
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds;

/// <summary>The Else command.</summary>
public class ElseCommand: AbstractCommand
{
    // <--[command]
    // @Name else
    // @Arguments ['if' <comparisons>]
    // @Short Executes the following block of commands only if the previous if failed, and optionally if additional requirements are met.
    // @Updated 2016/04/27
    // @Authors mcmonkey
    // @Group Queue
    // @Block Always
    // @Minimum 0
    // @Maximum -1
    // @Description
    // Executes the following block of commands only if the previous if failed, and optionally if additional requirements are met.
    // Works with the <@link command if>if command<@/link>.
    // TODO: Explain more!
    // @Example
    // // This example echos "hi".
    // if false
    // {
    //     echo "nope";
    // }
    // else
    // {
    //     echo "hi";
    // }
    // @Example
    // // TODO: More examples!
    // -->

    /// <summary>Constructs the else command.</summary>
    public ElseCommand()
    {
        Name = "else";
        Arguments = "['if' <comparisons>]";
        Description = "Executes the following block of commands only if the previous if failed, and optionally if additional requirements are met.";
        IsFlow = true;
        Asyncable = true;
        MinimumArguments = 0;
        MaximumArguments = -1;
        ObjectTypes =
        [
            Verify
        ];
    }

    void Verify(ArgumentValidation validator)
    {
        if (validator.ObjectValue.ToString().ToLowerFast() == "if")
        {
            validator.ObjectValue = new TextTag("if");
        }
        else
        {
            validator.ErrorResult = "Input to first argument must be 'if' (or nothing).";
        }
    }

    /// <summary>Represents the "TryIfCIL(queue, entry)" method.</summary>
    public static MethodInfo TryIfCILMethod = typeof(ElseCommand).GetMethod("TryIfCIL", new Type[] { typeof(CommandQueue), typeof(CommandEntry) });

    /// <summary>Adapts a command entry to CIL.</summary>
    /// <param name="values">The adaptation-relevant values.</param>
    /// <param name="entry">The present entry ID.</param>
    public override void AdaptToCIL(CILAdaptationValues values, int entry)
    {
        CommandEntry cent = values.Entry.Entries[entry];
        if (cent.IsCallback)
        {
            values.MarkCommand(entry);
            // TODO: Debug?
            int dodgepoint = cent.BlockEnd + 2;
            while (dodgepoint < values.Entry.Entries.Length)
            {
                CommandEntry tent = values.Entry.Entries[dodgepoint];
                if (tent.Command is ElseCommand)
                {
                    dodgepoint = tent.BlockEnd + 2;
                }
                else
                {
                    break;
                }
            }
            values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[dodgepoint]);
            return;
        }
        if (cent.BlockEnd <= 0)
        {
            throw new Exception("Incorrectly defined Else command: no block follows!");
        }
        if (cent.Arguments.Length > 0 && cent.Arguments[0].ToString() != "if")
        {
            throw new Exception("Incorrectly defined Else command: argument input that isn't 'if'!");
        }
        values.MarkCommand(entry);
        values.LoadQueue();
        values.LoadEntry(entry);
        values.ILGen.Emit(OpCodes.Call, TryIfCILMethod);
        values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
    }

    /// <summary>Executes the command via CIL.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static bool TryIfCIL(CommandQueue queue, CommandEntry entry)
    {
        entry.SetData(queue, new IfCommandData() { Result = 0 });
        if (entry.Arguments.Length == 0)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Else is reached, executing block...");
            }
            ((IfCommandData)entry.GetData(queue)).Result = 1;
            return true;
        }
        List<Argument> args = new(entry.Arguments);
        args.RemoveAt(0);
        bool success = IfCommand.TryIf(queue, entry, args);
        if (success)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Else-If is true, executing...");
            }
            ((IfCommandData)entry.GetData(queue)).Result = 1;
        }
        else
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Else-If is false, doing nothing!");
            }
        }
        return success;
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        throw new NotImplementedException("Unknown execution of ELSE command, invalid!");
    }
}
