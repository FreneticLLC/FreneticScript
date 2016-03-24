﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class IfCommandData : AbstractCommandEntryData
    {
        public int Result;
        public override AbstractCommandEntryData Duplicate()
        {
            IfCommandData toret = new IfCommandData();
            toret.Result = Result;
            return toret;
        }
    }

    class IfCommand: AbstractCommand
    {
        // TODO: Meta!
        public IfCommand()
        {
            Name = "if";
            Arguments = "<comparisons>";
            Description = "Executes the following block of commands only if the input is true.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = -1;
        }

        public override void Execute(CommandEntry entry)
        {
            IfCommandData data = new IfCommandData();
            data.Result = 0;
            entry.Data = data;
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                return;
            }
            if (entry.Block == null)
            {
                entry.Error("If invalid: No block follows!");
                return;
            }
            List<string> parsedargs = new List<string>(entry.Arguments.Count);
            for (int i = 0; i < entry.Arguments.Count; i++)
            {
                parsedargs.Add(entry.GetArgument(i));
            }
            bool success = TryIf(parsedargs);
            if (success)
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("If is true, executing...");
                }
                data.Result = 1;
                entry.Block.Add(new CommandEntry("if \0CALLBACK", null, entry,
                    this, new List<Argument>() { CommandSystem.TagSystem.SplitToArgument("\0CALLBACK", true) }, "if", 0, entry.ScriptName, entry.ScriptLine));
                entry.Queue.AddCommandsNow(entry.Block);
            }
            else
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("If is false, doing nothing!");
                }
            }
        }

        public static bool TryIf(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return false;
            }
            if (arguments.Count == 1)
            {
                return arguments[0].ToLowerInvariant() == "true";
            }
            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i] == "(")
                {
                    List<string> subargs = new List<string>();
                    int count = 0;
                    bool found = false;
                    for (int x = i + 1; x < arguments.Count; x++)
                    {
                        if (arguments[x] == "(")
                        {
                            count++;
                            subargs.Add("(");
                        }
                        else if (arguments[x] == ")")
                        {
                            count--;
                            if (count == -1)
                            {
                                bool cfound = TryIf(subargs);
                                arguments.RemoveRange(i, (x - i) + 1);
                                arguments.Insert(i, cfound.ToString());
                                found = true;
                            }
                            else
                            {
                                subargs.Add(")");
                            }
                        }
                        else
                        {
                            subargs.Add(arguments[x]);
                        }
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
                else if (arguments[i] == ")")
                {
                    return false;
                }
            }
            if (arguments.Count == 1)
            {
                return arguments[0].ToLowerInvariant() == "true";
            }
            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i] == "||")
                {
                    List<string> beforeargs = new List<string>(i);
                    for (int x = 0; x < i; x++)
                    {
                        beforeargs.Add(arguments[x]);
                    }
                    bool before = TryIf(beforeargs);
                    List<string> afterargs = new List<string>(i);
                    for (int x = i + 1; x < arguments.Count; x++)
                    {
                        afterargs.Add(arguments[x]);
                    }
                    bool after = TryIf(afterargs);
                    return before || after;
                }
                else if (arguments[i] == "&&")
                {
                    List<string> beforeargs = new List<string>(i);
                    for (int x = 0; x < i; x++)
                    {
                        beforeargs.Add(arguments[x]);
                    }
                    bool before = TryIf(beforeargs);
                    List<string> afterargs = new List<string>(i);
                    for (int x = i + 1; x < arguments.Count; x++)
                    {
                        afterargs.Add(arguments[x]);
                    }
                    bool after = TryIf(afterargs);
                    return before && after;
                }
            }
            if (arguments.Count == 1)
            {
                return arguments[0].ToLowerInvariant() == "true";
            }
            if (arguments.Count == 2)
            {
                return false;
            }
            if (arguments[1] == "==")
            {
                return arguments[0] == arguments[2];
            }
            else if (arguments[1] == "!=")
            {
                return arguments[0] != arguments[2];
            }
            else if (arguments[1] == ">=")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) >= FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else if (arguments[1] == "<=")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) <= FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else if (arguments[1] == ">")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) > FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else if (arguments[1] == "<")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) < FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else
            {
                return false;
            }
        }
    }
}
