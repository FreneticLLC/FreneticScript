using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class GotoCommand : AbstractCommand
    {
        // TODO: Meta!
        public GotoCommand()
        {
            Name = "goto";
            Arguments = "<mark name>";
            Description = "Goes to the marked location in the script.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    return new TextTag(input.ToString());
                }
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string targ = entry.GetArgument(queue, 0);
            CommandStackEntry cse = queue.CommandStack.Peek();
            for (int i = 0; i < cse.Entries.Length; i++)
            {
                if (queue.GetCommand(i).Command is MarkCommand
                    && queue.GetCommand(i).Arguments[0].ToString() == targ)
                {
                    // TODO: Maybe parse tags in the mark commands?
                    cse.Index = i;
                    return;
                }
            }
            queue.HandleError(entry, "Cannot goto marked location: unknown marker!");
        }
    }
}
