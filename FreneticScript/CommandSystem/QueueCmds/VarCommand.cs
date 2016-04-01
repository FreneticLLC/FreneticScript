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
            Arguments = "<variable> <setter> <value>";
            Description = "Modifies a variable in the current queue.";
            IsFlow = true;
            MinimumArguments = 3;
            MaximumArguments = 3;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    return new TextTag(input.ToString());
                },
                (input) =>
                {
                    return new TextTag(input.ToString());
                },
                (input) =>
                {
                    return input;
                }
            };
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string variable = entry.GetArgument(queue, 0);
            TemplateObject varb = queue.GetVariable(variable);
            string setter = entry.GetArgument(queue, 1);
            TemplateObject value = entry.GetArgumentObject(queue, 2);
            CommandStackEntry cse = queue.CommandStack.Peek();
            // TODO: Fix the below
            TagData dat = new TagData(entry.Command.CommandSystem.TagSystem, new TagBit[0], TextStyle.Color_Simple, cse.Variables, cse.Debug, (o) => queue.HandleError(entry, o), null);
            switch (setter)
            {
                case "=":
                    queue.SetVariable(variable, value);
                    break;
                case "+=":
                    double added = NumberTag.For(dat, varb).Internal + NumberTag.For(dat, value).Internal;
                    queue.SetVariable(variable, new NumberTag(added));
                    break;
                case "-=":
                    double subbed = NumberTag.For(dat, varb).Internal - NumberTag.For(dat, value).Internal;
                    queue.SetVariable(variable, new NumberTag(subbed));
                    break;
                case "/=":
                    double divd = NumberTag.For(dat, varb).Internal / NumberTag.For(dat, value).Internal;
                    queue.SetVariable(variable, new NumberTag(divd));
                    break;
                case "*=":
                    double multd = NumberTag.For(dat, varb).Internal + NumberTag.For(dat, value).Internal;
                    queue.SetVariable(variable, new NumberTag(multd));
                    break;
                case ".=":
                    string combined = varb.ToString() + value.ToString();
                    queue.SetVariable(variable, new TextTag(combined));
                    break;
                default:
                    queue.HandleError(entry, "Invalid setter!");
                    return;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Variable updated!");
            }
        }
    }
}
