using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class WaitCommand: AbstractCommand
    {
        // TODO: Meta!
        public WaitCommand()
        {
            Name = "wait";
            Arguments = "<time to wait in seconds>";
            Description = "Delays the current command queue a specified amount of time.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    return NumberTag.TryFor(input);
                }
            };
        }

        public static float StringToFloat(string input)
        {
            float output = 0;
            float.TryParse(input, out output);
            return output;
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string delay = entry.GetArgument(queue, 0);
            float seconds = StringToFloat(delay);
            if (queue.Delayable)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Delaying for <{text_color.emphasis}>" + seconds + "<{text_color.base}> seconds.");
                }
                queue.Wait = seconds;
            }
            else
            {
                queue.HandleError(entry, "Cannot delay, inside an instant queue!");
            }
        }
    }
}
