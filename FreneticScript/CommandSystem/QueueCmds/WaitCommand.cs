using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem.QueueCmds
{
    public class WaitCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the wait command.
        /// </summary>
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
                NumberTag.TryFor
            };
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            base.AdaptToCIL(values, entry);
            values.ILGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Converts a string to a float.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The float.</returns>
        public static float StringToFloat(string input)
        {
            float.TryParse(input, out float output);
            return output;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string delay = entry.GetArgument(queue, 0);
            float seconds = StringToFloat(delay);
            if (queue.Delayable)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Delaying for <{text_color[emphasis]}>" + seconds + "<{text_color[base]}> seconds.");
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
