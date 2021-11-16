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

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>The Reload command: reloads the system.</summary>
    public class ReloadCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>Constructs the reload command.</summary>
        public ReloadCommand()
        {
            Name = "reload";
            Arguments = "";
            Description = "Reloads the command engine.";
            MinimumArguments = 0;
            MaximumArguments = 0;
        }

        /// <summary>Executes the command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Reloading...");
            }
            queue.Engine.Reload();
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Reloaded!");
            }
        }
    }
}
