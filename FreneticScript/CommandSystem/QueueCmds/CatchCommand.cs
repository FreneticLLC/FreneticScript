using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class CatchCommand: AbstractCommand
    {
        // TODO: Meta!
        public CatchCommand()
        {
            Name = "catch";
            Arguments = "";
            Description = "Contains exception handling code, only allowed to follow a try block.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TagHandlers.TemplateObject, TagHandlers.TemplateObject>>();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.Arguments[0].ToString() == "\0CALLBACK")
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
