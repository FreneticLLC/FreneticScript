using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.CommandSystem.QueueCmds
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
                    double added = FreneticUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) + FreneticUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(added.ToString()));
                    break;
                case "-=":
                    double subbed = FreneticUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) - FreneticUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(subbed.ToString()));
                    break;
                case "/=":
                    double divd = FreneticUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) / FreneticUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(divd.ToString()));
                    break;
                case "*=":
                    double multd = FreneticUtilities.StringToDouble(entry.Queue.GetVariable(variable).ToString()) + FreneticUtilities.StringToDouble(value);
                    entry.Queue.SetVariable(variable, new TextTag(multd.ToString()));
                    break;
                case ".=":
                    string combined = entry.Queue.GetVariable(variable).ToString() + value;
                    entry.Queue.SetVariable(variable, new TextTag(combined.ToString()));
                    break;
                default:
                    entry.Bad("Invalid setter!");
                    return;
            }
            entry.Good("Variable updated!");
        }
    }
}
