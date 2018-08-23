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
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>
    /// A non-user-invocable command called when no other command exists.
    /// </summary>
    public class DebugOutputInvalidCommand: AbstractCommand
    {
        // Note: Intentionally no meta!

        /// <summary>
        /// Constructs the command.
        /// </summary>
        public DebugOutputInvalidCommand()
        {
            Name = "\0DebugOutputInvalidCommand";
            Arguments = "<invalid command name>";
            Description = "Reports that a command is invalid, or submits it to a server.";
            IsDebug = true;
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = -1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string name = entry.Arguments[0].ToString();
            List<string> args = new List<string>(entry.Arguments.Count);
            for (int i = 1; i < entry.Arguments.Count; i++)
            {
                args.Add(entry.Arguments[i].ToString());
            }
            queue.CommandSystem.Context.UnknownCommand(queue, name, args.ToArray());
        }
    }
}
