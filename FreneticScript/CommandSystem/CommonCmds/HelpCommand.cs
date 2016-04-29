using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class HelpCommand : AbstractCommand
    {
        // TODO: Meta!

        public HelpCommand()
        {
            Name = "help";
            Description = "Shows help information on any command.";
            Arguments = "<command name>";
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For
            };
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string cmd = entry.GetArgument(queue, 0);
            AbstractCommand acmd;
            if (!entry.Command.CommandSystem.RegisteredCommands.TryGetValue(cmd, out acmd))
            {
                queue.HandleError(entry, "Unrecognized command name!");
                return;
            }
            acmd.ShowUsage(queue, entry, false);
        }
    }
}
