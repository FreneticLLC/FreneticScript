using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Parsing command.
    /// </summary>
    public class ParsingCommand : AbstractCommand
    {
        // <--[command]
        // @Name parsing
        // @Arguments 'on'/'off'
        // @Short Sets whether the current queue should parse tags.
        // @Updated 2016/04/28
        // @Authors mcmonkey
        // @Group Queue
        // @Minimum 1
        // @Maximum 1
        // @Description
        // Sets whether the current queue should parse tags.
        // TODO: Explain more!
        // @Example
        // // This example echos "<{wow!}>".
        // parsing off;
        // echo "<{wow!}>";
        // @Example
        // // TODO: More examples!
        // -->

        /// <summary>
        /// Constructs the parsing command.
        /// </summary>
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
                Verify
            };
            // TODO: Should this command exist?
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "on" || inp == "off")
            {
                return new TextTag(inp);
            }
            return null;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            TagParseMode modechoice = (TagParseMode)Enum.Parse(typeof(TagParseMode), entry.GetArgument(queue, 0).ToUpper());
            queue.ParseTags = modechoice;
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Queue parsing now <{text_color[emphasis]}>" + modechoice + "<{text_color[base]}>.");
            }
        }
    }
}
