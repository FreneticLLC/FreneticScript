using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }

        public override void Execute(CommandEntry entry)
        {
            string count = entry.GetArgument(0);
            if (count == "\0CALLBACK")
            {
                WhileCommandData dat = (WhileCommandData)entry.Queue.CommandList[entry.BlockStart - 1].Data;
                dat.Index++;
                List<string> comp = new List<string>();
                for (int i = 0; i < dat.ComparisonArgs.Count; i++)
                {
                    comp.Add(dat.ComparisonArgs[i].Parse(TextStyle.Color_Simple /* TODO: READ COLOR OFF QUEUE OR ENTRY */, entry.Queue.Variables, entry.Queue.Debug, entry.Error).ToString());
                }
                if (IfCommand.TryIf(comp))
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("While looping...: " + dat.Index);
                    }
                    entry.Queue.CommandIndex = entry.BlockStart;
                }
                if (entry.ShouldShowGood())
                {
                    entry.Good("While stopping.");
                }
            }
            else if (count.ToLowerInvariant() == "stop")
            {
                for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                {
                    if (entry.Queue.GetCommand(i).Command is WhileCommand &&
                        entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Stopping a while loop.");
                        }
                        entry.Queue.CommandIndex = i + 2;
                        break;
                    }
                }
                entry.Error("Cannot stop while: not in one!");
            }
            else if (count.ToLowerInvariant() == "next")
            {
                for (int i = entry.Queue.CommandIndex - 1; i > 0; i--)
                {
                    if (entry.Queue.GetCommand(i).Command is WhileCommand &&
                        entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Jumping forward in a while loop.");
                        }
                        entry.Queue.CommandIndex = i + 1;
                        break;
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
                    entry.Queue.CommandIndex = entry.BlockEnd + 2;
                    return;
                }
                entry.Data = new WhileCommandData() { Index = 1, ComparisonArgs = entry.Arguments };
            }
        }
    }
}
