using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;

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
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            string name = entry.Arguments[0].ToString();
            List<string> args = new List<string>(entry.Arguments.Count);
            for (int i = 1; i < entry.Arguments.Count; i++)
            {
                args.Add(entry.Arguments[i].ToString());
            }
            entry.Output.UnknownCommand(entry.Queue, name, args.ToArray());
        }
    }
}
