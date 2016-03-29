using System;
using System.Collections.Generic;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name function
    // @Arguments 'stop'/'define'/'undefine' [name of function] ['quiet_fail']
    // @Short Creates a new function of the following command block, and adds it to the script cache.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Braces allowed
    // @Minimum 1
    // @Maximum 3
    // @Description
    // The function command will define the included command block to be a function which can be activated
    // by the <@link command call>call<@/link> command.
    // Add the 'quiet_fail' argument to not produce an error message if the function already exists.
    // Use "function stop" inside a function to end the function call without killing the queue that started it.
    // TODO: Explain more!
    // @Example
    // // This example creates function "helloworld" which, when called, outputs "hello world", then stops before it can output a "!"
    // function define helloworld
    // {
    //     echo "hello world"
    //     function stop
    //     echo "!"
    // }
    // @Example
    // // This example creates function "outputme" which, when called with variable 'text', outputs the specified text.
    // function define outputme
    // {
    //     require text
    //     echo <{var[text]}>
    // }
    // @Example
    // TODO: More examples!
    // -->
    class FunctionCommand : AbstractCommand
    {
        public override void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            entry.BlockEnd -= input.Count;
            input.Clear();
            base.AdaptBlockFollowers(entry, input, fblock);
            fblock.Add(GetFollower(entry));
        }

        public FunctionCommand()
        {
            Name = "function";
            Arguments = "'stop'/'define' [name of function] ['quiet_fail']";
            Description = "Creates a new function of the following command block, and adds it to the script cache.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 3;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    if (input.ToString() == "\0CALLBACK")
                    {
                        return input;
                    }
                    string inp = input.ToString().ToLowerFast();
                    if (inp == "stop" || inp == "define" || inp == "undefine")
                    {
                        return new TextTag(inp);
                    }
                    return null;
                },
                (input) =>
                {
                    return new TextTag(input.ToString());
                },
                (input) =>
                {
                    return new TextTag(input.ToString());
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            string type = entry.GetArgument(0);
            if (type == "\0CALLBACK")
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Completed function call.");
                }
                return;
            }
            type = type.ToLowerFast();
            if (type == "stop")
            {
                for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                {
                    if (entry.Queue.GetCommand(i).Command is FunctionCommand && entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Stopping a function call.");
                        }
                        entry.Queue.CommandIndex = i + 2;
                        return;
                    }
                }
                entry.Error("Cannot stop function: not in one!");
            }
            else if (type == "undefine")
            {
                if (entry.Arguments.Count < 2)
                {
                    ShowUsage(entry);
                    return;
                }
                string name = entry.GetArgument(1).ToLowerFast();
                if (!entry.Queue.CommandSystem.Functions.ContainsKey(name))
                {
                    if (entry.Arguments.Count > 2 && entry.GetArgument(2).ToLowerFast() == "quiet_fail")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Function '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' doesn't exist!");
                        }
                    }
                    else
                    {
                        entry.Error("Function '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' doesn't exist!");
                    }
                }
                else
                {
                    entry.Queue.CommandSystem.Functions.Remove(name);
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Function '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' undefined.");
                    }
                }
            }
            else if (type == "define")
            {
                if (entry.Arguments.Count < 2)
                {
                    ShowUsage(entry);
                    return;
                }
                string name = entry.GetArgument(1).ToLowerFast();
                if (entry.InnerCommandBlock == null)
                {
                    entry.Error("Function invalid: No block follows!");
                    return;
                }
                if (entry.Queue.CommandSystem.Functions.ContainsKey(name))
                {
                    if (entry.Arguments.Count > 2 && entry.GetArgument(2).ToLowerFast() == "quiet_fail")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Function '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' already exists!");
                        }
                    }
                    else
                    {
                        entry.Error("Function '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' already exists!");
                    }
                }
                else
                {
                    entry.Queue.CommandSystem.Functions.Add(name, new CommandScript("function_" + name, entry.InnerCommandBlock, entry.BlockStart));
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Function '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' defined.");
                    }
                }
                entry.Queue.CommandIndex = entry.BlockEnd + 2;
            }
            else
            {
                ShowUsage(entry);
            }
        }
    }
}
