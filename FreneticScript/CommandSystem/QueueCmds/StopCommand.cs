using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // TODO: Meta!

    public class StopCommand : AbstractCommand
    {
        public StopCommand()
        {
            Name = "stop";
            Arguments = "['all']";
            Description = "Stops the current command queue.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "all")
            {
                return new TextTag(inp);
            }
            return null;
        }

        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.GetArgument(queue, 0).ToLowerFast() == "all")
            {
                int qCount = queue.CommandSystem.Queues.Count;
                if (!queue.CommandSystem.Queues.Contains(queue))
                {
                    qCount++;
                }
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Stopping <{text_color[emphasis]}>" + qCount + "<{text_color[base]}> queue" + (qCount == 1 ? "." : "s."));
                }
                foreach (CommandQueue tqueue in queue.CommandSystem.Queues)
                {
                    tqueue.Stop();
                }
                queue.Stop();
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Stopping current queue.");
                }
                queue.Stop();
            }
        }
    }
}
