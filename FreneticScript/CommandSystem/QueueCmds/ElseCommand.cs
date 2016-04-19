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

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                CommandStackEntry cse = queue.CommandStack.Peek();
                CommandEntry ifentry = cse.Entries[entry.BlockStart - 1];
                if (cse.Index + 1 < cse.Entries.Length)
                {
                    CommandEntry elseentry = cse.Entries[cse.Index + 1];
                    if (elseentry.Command is ElseCommand)
                    {
                        elseentry.SetData(queue, ifentry.GetData(queue));
                    }
                }
                return;
            }
            if (!(entry.GetData(queue) is IfCommandData))
            {
                queue.HandleError(entry, "ELSE invalid, IF did not preceed!");
                return;
            }
            IfCommandData data = (IfCommandData)entry.GetData(queue);
            if (data.Result == 1)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Else continuing, previous IF passed.");
                }
                queue.CommandStack.Peek().Index = entry.BlockEnd + 1;
                return;
            }
            bool success = true;
            if (entry.Arguments.Count >= 1)
            {
                string ifbit = entry.GetArgument(queue, 0);
                if (ifbit.ToLowerFast() != "if")
                {
                    ShowUsage(queue, entry);
                    return;
                }
                else
                {
                    List<string> parsedargs = new List<string>(entry.Arguments.Count);
                    for (int i = 1; i < entry.Arguments.Count; i++)
                    {
                        parsedargs.Add(entry.GetArgument(queue, i)); // TODO: Don't pre-parse. Parse in TryIf.
                    }
                    success = IfCommand.TryIf(parsedargs);
                }
            }
            if (success)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Else [if] is true, executing...");
                }
                data.Result = 1;
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Else continuing, ELSE-IF is false!");
                }
                queue.CommandStack.Peek().Index = entry.BlockEnd + 1;
            }
        }
    }
}
