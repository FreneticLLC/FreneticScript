using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name function
    // @Arguments stop/define [name of function] (quiet_fail)
    // @Short Creates a new function of the following command block, and adds it to the script cache.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Braces true
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
        public FunctionCommand()
        {
            Name = "function";
            Arguments = "stop/define [name of function] (quiet_fail)";
            Description = "Creates a new function of the following command block, and adds it to the script cache.";
            IsFlow = true;
            Asyncable = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string type = entry.GetArgument(0).ToLowerInvariant();
            if (type == "stop")
            {
                bool hasnext = false;
                for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                {
                    if (entry.Queue.GetCommand(i).CommandLine == "call \0CALLBACK")
                    {
                        hasnext = true;
                        break;
                    }
                }
                if (hasnext)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Stopping function call.");
                    }
                    while (entry.Queue.CommandList.Length > 0)
                    {
                        if (entry.Queue.GetCommand(0).CommandLine == "call \0CALLBACK")
                        {
                            entry.Queue.RemoveCommand(0);
                            break;
                        }
                        entry.Queue.RemoveCommand(0);
                    }
                }
                else
                {
                    entry.Error("Cannot stop function call: not in one!");
                }
                return;
            }
            else if (type == "define")
            {
                if (entry.Arguments.Count < 2)
                {
                    ShowUsage(entry);
                    return;
                }
                string name = entry.GetArgument(1).ToLowerInvariant();
                if (entry.Block == null)
                {
                    entry.Error("Function invalid: No block follows!");
                    return;
                }
                if (entry.Queue.CommandSystem.Functions.ContainsKey(name))
                {
                    if (entry.Arguments.Count > 2 && entry.GetArgument(2).ToLowerInvariant() == "quiet_fail")
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
                    entry.Queue.CommandSystem.Functions.Add(name, new CommandScript(name, CommandScript.DisOwn(entry.Block, entry)));
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Function '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' defined.");
                    }
                }
            }
            else
            {
                ShowUsage(entry);
            }
        }
    }
}
