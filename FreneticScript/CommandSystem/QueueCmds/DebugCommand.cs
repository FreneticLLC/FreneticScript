using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class DebugCommand: AbstractCommand
    {
        public DebugCommand()
        {
            Name = "debug";
            Arguments = "full/minimal/none";
            Description = "Modifies the debug mode of the current queue.";
            IsFlow = true;
            Asyncable = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                string modechoice = entry.GetArgument(0);
                switch (modechoice.ToLower())
                {
                    case "full":
                        entry.Queue.Debug = DebugMode.FULL;
                        entry.Good("Queue debug mode set to <{text_color.emphasis}>full<{text_color.base}>.");
                        break;
                    case "minimal":
                        entry.Queue.Debug = DebugMode.MINIMAL;
                        entry.Good("Queue debug mode set to <{text_color.emphasis}>minimal<{text_color.base}>.");
                        break;
                    case "none":
                        entry.Queue.Debug = DebugMode.NONE;
                        entry.Good("Queue debug mode set to <{text_color.emphasis}>none<{text_color.base}>.");
                        break;
                    default:
                        entry.Error("Unknown debug mode '<{text_color.emphasis}>" + modechoice + "<{text_color.base}>'.");
                        break;
                }
            }
        }
    }
}
