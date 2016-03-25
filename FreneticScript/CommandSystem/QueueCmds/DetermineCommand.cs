using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class DetermineCommand: AbstractCommand
    {
        // TODO: Meta!
        public DetermineCommand()
        {
            Name = "determine";
            Arguments = "[value to set on the queue]";
            Description = "Sets the value determined on the queue.";
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

        public override void Execute(CommandEntry entry)
        {
            TemplateObject determ = entry.GetArgumentObject(0);
            entry.Queue.Determinations.Add(determ);
            if (entry.ShouldShowGood())
            {
                entry.Good("<{color.info}>Determination of the queue set to '<{color.emphasis}>" + TagParser.Escape(determ.ToString()) + "<{color.info}>'.");
            }
        }
    }
}
