//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds;

/// <summary>The Goto command.</summary>
public class GotoCommand : AbstractCommand
{
    // <--[command]
    // @Name goto
    // @Arguments <mark name>
    // @Short Goes to the marked location in a script.
    // @Updated 2016/04/28
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 1
    // @Maximum 1
    // @Description
    // Goes to the marked location in a script.
    // See the <@link command mark>mark command<@/link>.
    // TODO: Explain more!
    // @Example
    // // This example echos "hi".
    // goto skip;
    // echo nope;
    // mark skip;
    // echo hi;
    // @Example
    // // TODO: More examples!
    // -->

    /// <summary>Constructs the goto command.</summary>
    public GotoCommand()
    {
        Name = "goto";
        Arguments = "<mark name>";
        Description = "Goes to the marked location in the script.";
        IsFlow = true;
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = 1;
        ObjectTypes =
        [
            TextTag.Validator
        ];
    }

    /// <summary>Adapts a command entry to CIL.</summary>
    /// <param name="values">The adaptation-relevant values.</param>
    /// <param name="entry">The present entry ID.</param>
    public override void AdaptToCIL(CILAdaptationValues values, int entry)
    {
        CommandEntry cent = values.Entry.Entries[entry];
        string targ = cent.Arguments[0].ToString();
        for (int i = 0; i < values.Entry.Entries.Length; i++)
        {
            if (values.Entry.Entries[i].Command is MarkCommand
                && values.Entry.Entries[i].Arguments[0].ToString() == targ)
            {
                // TODO: call Outputter function?
                values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[i]);
                return;
            }
        }
        throw new Exception("GOTO command invalid: no matching mark!");
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        throw new NotImplementedException("Must be compiled!");
    }
}
