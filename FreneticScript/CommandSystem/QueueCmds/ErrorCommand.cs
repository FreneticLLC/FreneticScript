using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // TODO: Meta!
    class ErrorCommand : AbstractCommand
    {
        public ErrorCommand()
        {
            Name = "error";
            Arguments = "<error message>";
            Description = "Throws an error on the current command queue.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            entry.Error(entry.GetArgument(0));
        }
    }
}
