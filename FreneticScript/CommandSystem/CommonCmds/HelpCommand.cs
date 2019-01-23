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
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>
    /// The Help command: shows help information on commands.
    /// </summary>
    public class HelpCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the help command.
        /// </summary>
        public HelpCommand()
        {
            Name = "help";
            Description = "Shows help information on any command.";
            Arguments = "<command name>";
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string cmd = entry.GetArgument(queue, 0);
            if (!entry.Command.CommandSystem.RegisteredCommands.TryGetValue(cmd, out AbstractCommand acmd))
            {
                queue.HandleError(entry, "Unrecognized command name!");
                return;
            }
            ShowUsage(queue, entry, false, acmd);
        }
    }
}
