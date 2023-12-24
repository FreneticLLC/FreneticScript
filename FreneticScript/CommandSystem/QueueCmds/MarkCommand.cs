//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds;

/// <summary>The Mark command.</summary>
public class MarkCommand : AbstractCommand
{
    // <--[command]
    // @Name mark
    // @Arguments <mark name>
    // @Short Marks a location in the script for the goto command.
    // @Updated 2016/04/28
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 1
    // @Maximum 1
    // @Description
    // Marks a location in the script for the goto command.
    // See the <@link command goto>goto command<@/link>.
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

    /// <summary>Constructs the mark command.</summary>
    public MarkCommand()
    {
        Name = "mark";
        Arguments = "<mark name>";
        Description = "Marks a location in the script for the goto command.";
        IsFlow = true;
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = 1;
        ObjectTypes =
        [
            TextTag.Validator
        ];
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "Passing mark.");
        }
    }
}
