using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    public class TryCommand : AbstractCommand
    {
        // TODO: Meta!
        // @Block always

        /// <summary>
        /// Constructs the try command.
        /// </summary>
        public TryCommand()
        {
            Name = "try";
            Arguments = "";
            Description = "Executes the following block of commands and exits forcefully if there is an error.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>();
        }
        
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.GetArgument(queue, 0) == "\0CALLBACK")
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Block completed successfully!");
                }
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Trying block...");
                }
            }
        }
    }
}
