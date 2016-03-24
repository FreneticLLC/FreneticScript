using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem.QueueCmds
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
            MinimumArguments = 1;
            MaximumArguments = 1;
        }

        public static float StringToFloat(string input)
        {
            float output = 0;
            float.TryParse(input, out output);
            return output;
        }

        public override void Execute(CommandEntry entry)
        {
            string delay = entry.GetArgument(0);
            float seconds = StringToFloat(delay);
            if (entry.Queue.Delayable)
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Delaying for <{text_color.emphasis}>" + seconds + "<{text_color.base}> seconds.");
                }
                entry.Queue.Wait = seconds;
            }
            else
            {
                entry.Error("Cannot delay, inside an instant queue!");
            }
        }
    }
}
