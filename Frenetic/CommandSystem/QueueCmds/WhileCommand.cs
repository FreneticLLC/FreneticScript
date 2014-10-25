using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem.QueueCmds
{
    class WhileCommandData : AbstractCommandEntryData
    {
        public List<string> ComparisonArgs;
        public int Index;
        public override AbstractCommandEntryData Duplicate()
        {
            WhileCommandData toret = new WhileCommandData();
            toret.ComparisonArgs = new List<string>(ComparisonArgs);
            toret.Index = Index;
            return toret;
        }
    }

    class WhileCommand : AbstractCommand
    {
        public WhileCommand()
        {
            Name = "while";
            Arguments = "true/false/stop/next";
            Description = "Executes the following block of commands continuously until the argument is false.";
            IsFlow = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                string count = entry.GetArgument(0);
                if (count == "\0CALLBACK")
                {
                    if (entry.BlockOwner.Command.Name == "while" || entry.BlockOwner.Block == null || entry.BlockOwner.Block.Count == 0
                        || entry.BlockOwner.Block[entry.BlockOwner.Block.Count - 1] != entry)
                    {
                        WhileCommandData data = (WhileCommandData)entry.BlockOwner.Data;
                        data.Index++;
                        StringBuilder comp = new StringBuilder();
                        for (int i = 0; i < data.ComparisonArgs.Count; i++)
                        {
                            comp.Append(entry.Queue.CommandSystem.TagSystem.ParseTags(
                                data.ComparisonArgs[i], TextStyle.Color_Simple, entry.Queue.Variables, entry.Queue.Debug));
                            if (i + 1 < data.ComparisonArgs.Count)
                            {
                                comp.Append(" ");
                            }
                        }
                        if (IfCommand.TryIf(comp.ToString()))
                        {
                            entry.Good("While loop at index <{color.emphasis}>" + data.Index + "<{color.base}>...");
                            entry.Queue.SetVariable("while_index", data.Index.ToString());
                            entry.Queue.AddCommandsNow(entry.BlockOwner.Block);
                        }
                        else
                        {
                            entry.Good("While loop ending, reached 'false'.");
                        }
                    }
                    else
                    {
                        entry.Bad("While CALLBACK invalid: not a real callback!");
                    }
                }
                else if (count.ToLower() == "stop")
                {
                    bool hasnext = false;
                    for (int i = 0; i < entry.Queue.CommandList.Count; i++)
                    {
                        if (entry.Queue.GetCommand(i).CommandLine == "while \0CALLBACK")
                        {
                            hasnext = true;
                            break;
                        }
                    }
                    if (hasnext)
                    {
                        entry.Good("Stopping while loop.");
                        while (entry.Queue.CommandList.Count > 0)
                        {
                            if (entry.Queue.GetCommand(0).CommandLine == "while \0CALLBACK")
                            {
                                entry.Queue.RemoveCommand(0);
                                break;
                            }
                            entry.Queue.RemoveCommand(0);
                        }
                    }
                    else
                    {
                        entry.Bad("Cannot stop while: not in one!");
                    }
                }
                else if (count.ToLower() == "next")
                {
                    bool hasnext = false;
                    for (int i = 0; i < entry.Queue.CommandList.Count; i++)
                    {
                        if (entry.Queue.GetCommand(i).CommandLine == "while \0CALLBACK")
                        {
                            hasnext = true;
                            break;
                        }
                    }
                    if (hasnext)
                    {
                        entry.Good("Skipping to next repeat entry...");
                        while (entry.Queue.CommandList.Count > 0)
                        {
                            if (entry.Queue.GetCommand(0).CommandLine == "while \0CALLBACK")
                            {
                                break;
                            }
                            entry.Queue.RemoveCommand(0);
                        }
                    }
                    else
                    {
                        entry.Bad("Cannot stop while: not in one!");
                    }
                }
                else
                {
                    string target = count + " " + entry.AllArguments(1);
                    if (!IfCommand.TryIf(target))
                    {
                        entry.Good("Not looping.");
                        return;
                    }
                    WhileCommandData data = new WhileCommandData();
                    data.Index = 1;
                    data.ComparisonArgs = new List<string>(entry.Arguments);
                    entry.Data = data;
                    if (entry.Block != null)
                    {
                        entry.Good("While looping...");
                        CommandEntry callback = new CommandEntry("while \0CALLBACK", null, entry,
                            this, new List<string> { "\0CALLBACK" }, "while", 0);
                        entry.Block.Add(callback);
                        entry.Queue.SetVariable("while_index", "1");
                        entry.Queue.AddCommandsNow(entry.Block);
                    }
                    else
                    {
                        entry.Bad("While invalid: No block follows!");
                    }
                }
            }
        }
    }
}
