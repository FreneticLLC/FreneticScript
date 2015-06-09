using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;

namespace Frenetic.CommandSystem.QueueCmds
{
    class MarkCommand : AbstractCommand
    {
        public MarkCommand()
        {
            Name = "mark";
            Arguments = "<mark name>";
            Description = "Marks a location in the script for the goto command.";
            IsFlow = true;
            Asyncable = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
        }
    }
}
