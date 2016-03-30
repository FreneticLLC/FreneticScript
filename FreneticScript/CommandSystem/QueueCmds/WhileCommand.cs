using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class WhileCommandData : AbstractCommandEntryData
    {
        public List<Argument> ComparisonArgs;
        public int Index;
    }

    class WhileCommand : AbstractCommand
    {
        // TODO: Meta!

        public WhileCommand()
        {
            Name = "while";
            Arguments = "'stop'/'next'/<comparisons>";
            Description = "Executes the following block of commands continuously until the argument is false.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = -1;
            IsBreakable = true;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    return new TextTag(input.ToString());
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            string count = entry.GetArgument(0);
            if (count == "\0CALLBACK")
            {
                CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                WhileCommandData dat = (WhileCommandData)cse.Entries[entry.BlockStart - 1].Data;
                dat.Index++;
                List<string> comp = new List<string>();
                for (int i = 0; i < dat.ComparisonArgs.Count; i++)
                {
                    // TODO: Preparse arguments less!
                    comp.Add(dat.ComparisonArgs[i].Parse(TextStyle.Color_Simple /* TODO: READ COLOR OFF QUEUE OR ENTRY */, cse.Variables, cse.Debug, entry.Error).ToString());
                }
                if (IfCommand.TryIf(comp))
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("While looping...: " + dat.Index);
                    }
                    cse.Index = entry.BlockStart;
                    return;
                }
                if (entry.ShouldShowGood())
                {
                    entry.Good("While stopping.");
                }
            }
            else if (count.ToLowerFast() == "stop")
            {
                CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                for (int i = 0; i < cse.Entries.Length; i++)
                {
                    if (entry.Queue.GetCommand(i).Command is WhileCommand && entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Stopping a while loop.");
                        }
                        cse.Index = i + 2;
                        return;
                    }
                }
                entry.Error("Cannot stop while: not in one!");
            }
            else if (count.ToLowerFast() == "next")
            {
                CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                for (int i = cse.Index - 1; i > 0; i--)
                {
                    if (entry.Queue.GetCommand(i).Command is WhileCommand && entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Jumping forward in a while loop.");
                        }
                        cse.Index = i + 1;
                        return;
                    }
                }
                entry.Error("Cannot while repeat: not in one!");
            }
            else
            {
                List<string> parsedargs = new List<string>(entry.Arguments.Count + 1);
                parsedargs.Add(count);
                for (int i = 1; i < entry.Arguments.Count; i++)
                {
                    parsedargs.Add(entry.GetArgument(i));
                }
                bool success = IfCommand.TryIf(parsedargs);
                if (!success)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Not looping.");
                    }
                    CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                    cse.Index = entry.BlockEnd + 2;
                    return;
                }
                entry.Data = new WhileCommandData() { Index = 1, ComparisonArgs = entry.Arguments };
            }
        }
    }
}
