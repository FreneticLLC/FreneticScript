using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    public class ElseCommand: AbstractCommand
    {
        // <--[command]
        // @Name else
        // @Arguments ['if' <comparisons>]
        // @Short Executes the following block of commands only if the previous if failed, and optionally if additional requirements are met.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Group Queue
        // @Block Always
        // @Minimum 0
        // @Maximum -1
        // @Description
        // Executes the following block of commands only if the previous if failed, and optionally if additional requirements are met.
        // Works with the <@link command if>if command<@/link>.
        // TODO: Explain more!
        // @Example
        // // This example echos "hi".
        // if false
        // {
        //     echo "nope";
        // }
        // else
        // {
        //     echo "hi";
        // }
        // @Example
        // // TODO: More examples!
        // -->
        public ElseCommand()
        {
            Name = "else";
            Arguments = "['if' <comparisons>]";
            Description = "Executes the following block of commands only if the previous if failed, and optionally if additional requirements are met.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = -1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString();
            if (inp == "\0CALLBACK")
            {
                return input;
            }
            if (inp.ToLowerFast() == "if")
            {
                return new TextTag("if");
            }
            return null;
        }

        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                CommandStackEntry cse = queue.CurrentEntry;
                CommandEntry ifentry = cse.Entries[entry.BlockStart - 1];
                if (cse.Index < cse.Entries.Length)
                {
                    CommandEntry elseentry = cse.Entries[cse.Index];
                    if (elseentry.Command is ElseCommand)
                    {
                        elseentry.SetData(queue, ifentry.GetData(queue));
                    }
                }
                return;
            }
            if (!(entry.GetData(queue) is IfCommandData))
            {
                queue.HandleError(entry, "ELSE invalid, IF did not precede!");
                return;
            }
            IfCommandData data = (IfCommandData)entry.GetData(queue);
            if (data.Result == 1)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Else continuing, previous IF passed.");
                }
                queue.CurrentEntry.Index = entry.BlockEnd + 1;
                return;
            }
            bool success = true;
            if (entry.Arguments.Count >= 1)
            {
                success = IfCommand.TryIf(queue, entry, new List<Argument>(entry.Arguments));
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
                queue.CurrentEntry.Index = entry.BlockEnd + 1;
            }
        }
    }
}
