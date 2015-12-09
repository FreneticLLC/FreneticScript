using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem.QueueCmds
{
    class WaitCommand: AbstractCommand
    {
        public WaitCommand()
        {
            Name = "wait";
            Arguments = "<time to wait in seconds>";
            Description = "Delays the current command queue a specified amount of time.";
            IsFlow = true;
            Asyncable = true;
        }

        public static float StringToFloat(string input)
        {
            float output = 0;
            float.TryParse(input, out output);
            return output;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                string delay = entry.GetArgument(0);
                float seconds = StringToFloat(delay);
                if (entry.Queue.Delayable)
                {
                    entry.Good("Delaying for <{text_color.emphasis}>" + seconds + "<{text_color.base}> seconds.");
                    entry.Queue.Wait = seconds;
                }
                else
                {
                    entry.Bad("Cannot delay, inside an instant queue!");
                }
            }
        }
    }
}
