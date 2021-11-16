//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>The Try command.</summary>
    public class TryCommand : AbstractCommand
    {
        // TODO: Meta!
        // @Block always

        /// <summary>Constructs the try command.</summary>
        public TryCommand()
        {
            Name = "try";
            Arguments = "";
            Description = "Executes the following block of commands and exits forcefully if there is an error.";
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
                    entry.GoodOutput(queue, "Block completed successfully!");
                }
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Trying block...");
                }
            }
        }
    }
}
