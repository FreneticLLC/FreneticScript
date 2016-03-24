using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class DebugCommand : AbstractCommand
    {
        // TODO: Meta!
        public DebugCommand()
        {
            Name = "debug";
            Arguments = "'full'/'minimal'/'none'";
            Description = "Modifies the debug mode of the current queue.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    string inp = input.ToString().ToLowerFast();
                    if (inp == "full" || inp == "minimal" || inp == "none")
                    {
                        return new TextTag(inp);
                    }
                    return null;
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            string modechoice = entry.GetArgument(0);
            switch (modechoice.ToLowerFast())
            {
                case "full":
                    entry.Queue.Debug = DebugMode.FULL;
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Queue debug mode set to <{text_color.emphasis}>full<{text_color.base}>.");
                    }
                    break;
                case "minimal":
                    entry.Queue.Debug = DebugMode.MINIMAL;
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Queue debug mode set to <{text_color.emphasis}>minimal<{text_color.base}>.");
                    }
                    break;
                case "none":
                    entry.Queue.Debug = DebugMode.NONE;
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Queue debug mode set to <{text_color.emphasis}>none<{text_color.base}>.");
                    }
                    break;
                default:
                    entry.Error("Unknown debug mode '<{text_color.emphasis}>" + modechoice + "<{text_color.base}>'.");
                    break;
            }
        }
    }
}
