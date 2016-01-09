using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

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
            string setter = entry.GetArgument(1);
            string value = entry.GetArgument(2);
            switch (setter)
            {
                case "=":
                    entry.Queue.SetVariable(variable, new TextTag(value));
                    break;
                case "+=":
                    double added = FreneticScriptUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) + FreneticScriptUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(added.ToString()));
                    break;
                case "-=":
                    double subbed = FreneticScriptUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) - FreneticScriptUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(subbed.ToString()));
                    break;
                case "/=":
                    double divd = FreneticScriptUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) / FreneticScriptUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(divd.ToString()));
                    break;
                case "*=":
                    double multd = FreneticScriptUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) + FreneticScriptUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(multd.ToString()));
                    break;
                case ".=":
                    string combined = entry.Queue.GetVariable(variable).ToString() + value;
                    entry.Queue.SetVariable(variable, new TextTag(combined.ToString()));
                    break;
                default:
                    entry.Error("Invalid setter!");
                    return;
            }
            entry.Good("Variable updated!");
        }
    }
}
