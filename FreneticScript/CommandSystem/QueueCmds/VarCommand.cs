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
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count != 3)
            {
                ShowUsage(entry);
                return;
            }
            string variable = entry.GetArgument(0);
            TemplateObject varb = entry.Queue.GetVariable(variable);
            string setter = entry.GetArgument(1);
            TemplateObject value = entry.GetArgumentObject(2);
            // TODO: Fix the below
            TagData dat = new TagData(entry.Command.CommandSystem.TagSystem, (List<TagBit>)null, "^r^7", entry.Queue.Variables, entry.Queue.Debug, (o) => { throw new Exception("Tag Exception:" + o); });
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
            entry.Good("Variable updated!");
        }
    }
}
