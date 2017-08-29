//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
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
