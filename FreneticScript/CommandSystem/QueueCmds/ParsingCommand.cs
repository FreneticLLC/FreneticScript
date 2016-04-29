using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class ParsingCommand : AbstractCommand
    {
        // TODO: Meta!
        public ParsingCommand()
        {
            Name = "parsing";
            Arguments = "'on'/'off'";
            Description = "Sets whether the current queue should parse tags.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                verify
            };
        }

        TemplateObject verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "on" || inp == "off")
            {
                return new TextTag(inp);
            }
            return null;
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            TagParseMode modechoice = (TagParseMode)Enum.Parse(typeof(TagParseMode), entry.GetArgument(queue, 0).ToUpper());
            queue.ParseTags = modechoice;
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Queue parsing now <{text_color.emphasis}>" + modechoice + "<{text_color.base}>.");
            }
        }
    }
}
