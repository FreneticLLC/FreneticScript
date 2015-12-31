using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;
using Frenetic.CommandSystem.Arguments;

namespace Frenetic.CommandSystem.QueueCmds
{
    class WhileCommandData : AbstractCommandEntryData
    {
        public List<Argument> ComparisonArgs;
        public int Index;
        public override AbstractCommandEntryData Duplicate()
        {
            WhileCommandData toret = new WhileCommandData();
            toret.ComparisonArgs = new List<Argument>(ComparisonArgs);
            toret.Index = Index;
            return toret;
        }
    }

    class WhileCommand : AbstractCommand
    {
        public WhileCommand()
        {
            Name = "while";
            Arguments = "stop/next/<if calculations>";
            Description = "Executes the following block of commands continuously until the argument is false.";
            IsFlow = true;
            Asyncable = true;
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
                        List<string> comp = new List<string>();
                        for (int i = 0; i < data.ComparisonArgs.Count; i++)
                        {
                            comp.Add(data.ComparisonArgs[i].Parse(TextStyle.Color_Simple /* TODO: READ COLOR OFF QUEUE OR ENTRY */, entry.Queue.Variables, entry.Queue.Debug, entry.Error).ToString());
                        }
                        if (IfCommand.TryIf(comp))
                        {
                            entry.Good("While loop at index <{text_color.emphasis}>" + data.Index + "<{text_color.base}>...");
                            entry.Queue.SetVariable("while_index", new TextTag(data.Index.ToString()));
                            entry.Queue.AddCommandsNow(entry.BlockOwner.Block);
                        }
                        else
                        {
                            entry.Good("While loop ending, reached 'false'.");
                        }
                    }
                    else
                    {
                        entry.Error("While CALLBACK invalid: not a real callback!");
                    }
                }
                else if (count.ToLower() == "stop")
                {
                    bool hasnext = false;
                    for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                    {
                        if (entry.Queue.GetCommand(i).Command is WhileCommand &&
                            entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                        {
                            hasnext = true;
                            break;
                        }
                    }
                    if (hasnext)
                    {
                        entry.Good("Stopping while loop.");
                        while (entry.Queue.CommandList.Length > 0)
                        {
                            if (entry.Queue.GetCommand(0).Command is WhileCommand &&
                                entry.Queue.GetCommand(0).Arguments[0].ToString() == "\0CALLBACK")
                            {
                                entry.Queue.RemoveCommand(0);
                                break;
                            }
                            entry.Queue.RemoveCommand(0);
                        }
                    }
                    else
                    {
                        entry.Error("Cannot stop while: not in one!");
                    }
                }
                else if (count.ToLower() == "next")
                {
                    bool hasnext = false;
                    for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                    {
                        if (entry.Queue.GetCommand(i).Command is WhileCommand &&
                            entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                        {
                            hasnext = true;
                            break;
                        }
                    }
                    if (hasnext)
                    {
                        entry.Good("Skipping to next repeat entry...");
                        while (entry.Queue.CommandList.Length > 0)
                        {
                            if (entry.Queue.GetCommand(0).Command is WhileCommand &&
                                entry.Queue.GetCommand(0).Arguments[0].ToString() == "\0CALLBACK")
                            {
                                break;
                            }
                            entry.Queue.RemoveCommand(0);
                        }
                    }
                    else
                    {
                        entry.Error("Cannot stop while: not in one!");
                    }
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
                        entry.Good("Not looping.");
                        return;
                    }
                    WhileCommandData data = new WhileCommandData();
                    data.Index = 1;
                    data.ComparisonArgs = new List<Argument>(entry.Arguments);
                    entry.Data = data;
                    if (entry.Block != null)
                    {
                        entry.Good("While looping...");
                        CommandEntry callback = new CommandEntry("while \0CALLBACK", null, entry,
                            this, new List<Argument>() { CommandSystem.TagSystem.SplitToArgument("\0CALLBACK") }, "while", 0, entry.ScriptName, entry.ScriptLine);
                        entry.Block.Add(callback);
                        entry.Queue.SetVariable("while_index", new TextTag("1"));
                        entry.Queue.AddCommandsNow(entry.Block);
                    }
                    else
                    {
                        entry.Error("While invalid: No block follows!");
                    }
                }
            }
        }
    }
}
