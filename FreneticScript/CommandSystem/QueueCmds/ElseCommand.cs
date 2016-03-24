using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class ElseCommand: AbstractCommand
    {
        // TODO: Meta!
        public ElseCommand()
        {
            Name = "else";
            Arguments = "['if' <comparisons>]";
            Description = "Executes the following block of commands only if the previous if failed, and optionally if additional requirements are met.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = -1;
            ObjectTypes = new List<Func<TagHandlers.TemplateObject, TagHandlers.TemplateObject>>();
        }

        public override void Execute(CommandEntry entry)
        {
            if (!(entry.Data is IfCommandData))
            {
                entry.Error("ELSE invalid, IF did not preceed!");
                return;
            }
            IfCommandData data = (IfCommandData)entry.Data;
            if (data.Result == 1)
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Else continuing, previous IF passed.");
                }
                entry.Queue.CommandIndex = entry.BlockEnd + 1;
                return;
            }
            bool success = true;
            if (entry.Arguments.Count >= 1)
            {
                string ifbit = entry.GetArgument(0);
                if (ifbit.ToLowerInvariant() != "if")
                {
                    ShowUsage(entry);
                    return;
                }
                else
                {
                    List<string> parsedargs = new List<string>(entry.Arguments.Count);
                    for (int i = 1; i < entry.Arguments.Count; i++)
                    {
                        parsedargs.Add(entry.GetArgument(i)); // TODO: Don't pre-parse. Parse in TryIf.
                    }
                    success = IfCommand.TryIf(parsedargs);
                }
            }
            if (success)
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Else [if] is true, executing...");
                }
                data.Result = 1;
            }
            else
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Else continuing, ELSE-IF is false!");
                }
                entry.Queue.CommandIndex = entry.BlockEnd + 1;
            }
        }
    }
}
