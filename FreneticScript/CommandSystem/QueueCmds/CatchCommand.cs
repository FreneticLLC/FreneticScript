using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class CatchCommand: AbstractCommand
    {
        public CatchCommand()
        {
            Name = "catch";
            Arguments = "";
            Description = "Contains exception handling code, only allowed to follow a try block.";
            IsFlow = true;
            Asyncable = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.GetArgument(0) == "\0CALLBACK")
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Completed catch successfully.");
                }
                return;
            }
            if (entry.ShouldShowGood())
            {
                entry.Good("Passing catch without executing.");
            }
        }
    }
}
