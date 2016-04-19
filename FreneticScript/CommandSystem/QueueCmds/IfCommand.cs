using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class IfCommandData : AbstractCommandEntryData
    {
        public int Result;
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
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>();
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                CommandStackEntry cse = queue.CommandStack.Peek();
                CommandEntry ifentry = cse.Entries[entry.BlockStart - 1];
                entry.SetData(queue, ifentry.GetData(queue));
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
            entry.SetData(queue, new IfCommandData() { Result = 0 });
            List<string> parsedargs = new List<string>(entry.Arguments.Count);
            for (int i = 0; i < entry.Arguments.Count; i++)
            {
                parsedargs.Add(entry.GetArgument(queue, i)); // TODO: Don't pre-parse. Parse in TryIf.
            }
            bool success = TryIf(parsedargs);
            if (success)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "If is true, executing...");
                }
                ((IfCommandData)entry.GetData(queue)).Result = 1;
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "If is false, doing nothing!");
                }
                queue.CommandStack.Peek().Index = entry.BlockEnd + 1;
            }
        }

        // TODO: better comparison system!
        public static bool TryIf(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return false;
            }
            if (arguments.Count == 1)
            {
                return arguments[0].ToLowerFast() == "true";
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
                return arguments[0].ToLowerFast() == "true";
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
                return arguments[0].ToLowerFast() == "true";
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
