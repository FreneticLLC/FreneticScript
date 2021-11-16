//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>The Catch command.</summary>
    public class CatchCommand: AbstractCommand
    {
        // <--[command]
        // @Name catch
        // @Arguments
        // @Short Contains exception handling code, only allowed to follow a <@link command try>try<@/link> block.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Group Queue
        // @Block Always
        // @Minimum 0
        // @Maximum 1
        // @Description
        // Contains exception handling code, only allowed to follow a <@link command try>try<@/link> block.
        // TODO: Explain more!
        // @Example
        // // This example catches errors and echoes them to the console.
        // try
        // {
        //     error "Hi there!";
        // }
        // catch
        // {
        //     echo "ERROR: <{[error_message]}>";
        // }
        // @Example
        // // TODO: More examples!
        // @Var stack_trace TextTag The error message that was caught, with full script tracing information.
        // -->

        /// <summary>Constructs the catch command.</summary>
        public CatchCommand()
        {
            Name = "catch";
            Arguments = "";
            Description = "Contains exception handling code, only allowed to follow a try block.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 0;
        }

        /// <summary>Executes the command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.IsCallback)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Completed catch successfully.");
                }
                return;
            }
            queue.CurrentRunnable.Index = entry.BlockEnd + 2;
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Passing catch without executing.");
            }
        }
    }
}
