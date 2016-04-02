using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class VarCommand : AbstractCommand // TODO: Public
    {
        // TODO: Meta
        public VarCommand()
        {
            Name = "var";
            Arguments = "<variable> '=' <value> ['as' <type>]";
            Description = "Modifies a variable in the current queue.";
            IsFlow = true;
            MinimumArguments = 3;
            MaximumArguments = 5;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    return new TextTag(input.ToString());
                },
                (input) =>
                {
                    if (input.ToString() == "=")
                    {
                        return new TextTag("=");
                    }
                    return null;
                },
                (input) =>
                {
                    return input;
                },
                (input) =>
                {
                    if (input.ToString() == "as")
                    {
                        return new TextTag("as");
                    }
                    return null;
                },
                (input) =>
                {
                    return input; // TODO: TagTypeTag?
                }
            };
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string variable = entry.GetArgument(queue, 0);
            TemplateObject varb = queue.GetVariable(variable);
            string setter = entry.GetArgument(queue, 1);
            TemplateObject value = entry.GetArgumentObject(queue, 2);
            if (entry.Arguments.Count == 5)
            {
                // TODO: Fix this tagdata nonsense
                TagData dat = new TagData(entry.Command.CommandSystem.TagSystem, new TagBit[0], TextStyle.Color_Simple, queue.CurrentEntry.Variables, queue.CurrentEntry.Debug, (o) => queue.HandleError(entry, o), null);
                TagTypeTag type = TagTypeTag.For(dat, entry.GetArgumentObject(queue, 4));
                TemplateObject obj = type.Internal.TypeGetter(dat, value);
                if (obj == null)
                {
                    queue.HandleError(entry, "Invalid object for specified type!");
                    return;
                }
                queue.SetVariable(variable, obj);
            }
            else
            {
                queue.SetVariable(variable, value);
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Variable updated!");
            }
        }
    }
}
