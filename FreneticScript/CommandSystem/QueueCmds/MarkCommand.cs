using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class MarkCommand : AbstractCommand
    {
        // TODO: Meta!
        public MarkCommand()
        {
            Name = "mark";
            Arguments = "<mark name>";
            Description = "Marks a location in the script for the goto command.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 2;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.ShouldShowGood() && (entry.Arguments.Count < 1 || entry.Arguments[1].ToString() == "\0SILENT"))
            {
                entry.Good("Passing mark.");
            }
        }
    }
}
