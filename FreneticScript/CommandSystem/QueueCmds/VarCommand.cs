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

        public override void Execute(CommandEntry entry)
        {
            string variable = entry.GetArgument(0);
            TemplateObject varb = entry.Queue.GetVariable(variable);
            string setter = entry.GetArgument(1);
            TemplateObject value = entry.GetArgumentObject(2);
            // TODO: Fix the below
            TagData dat = new TagData(entry.Command.CommandSystem.TagSystem, (List<TagBit>)null, TextStyle.Color_Simple, entry.Queue.Variables, entry.Queue.Debug, entry.Error, null);
            switch (setter)
            {
                case "=":
                    entry.Queue.SetVariable(variable, value);
                    break;
                case "+=":
                    double added = NumberTag.For(dat, varb).Internal + NumberTag.For(dat, value).Internal;
                    entry.Queue.SetVariable(variable, new NumberTag(added));
                    break;
                case "-=":
                    double subbed = NumberTag.For(dat, varb).Internal - NumberTag.For(dat, value).Internal;
                    entry.Queue.SetVariable(variable, new NumberTag(subbed));
                    break;
                case "/=":
                    double divd = NumberTag.For(dat, varb).Internal / NumberTag.For(dat, value).Internal;
                    entry.Queue.SetVariable(variable, new NumberTag(divd));
                    break;
                case "*=":
                    double multd = NumberTag.For(dat, varb).Internal + NumberTag.For(dat, value).Internal;
                    entry.Queue.SetVariable(variable, new NumberTag(multd));
                    break;
                case ".=":
                    string combined = varb.ToString() + value.ToString();
                    entry.Queue.SetVariable(variable, new TextTag(combined));
                    break;
                default:
                    entry.Error("Invalid setter!");
                    return;
            }
            if (entry.ShouldShowGood())
            {
                entry.Good("Variable updated!");
            }
        }
    }
}
