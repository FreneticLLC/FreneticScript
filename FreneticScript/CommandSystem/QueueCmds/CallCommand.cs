using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name call
    // @Arguments [inject/run] <function to call> [<variable>:<value> ...]
    // @Short Runs a function.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Description
    // Activates a function created by the <@link command function>function<@/link> command.
    // Note that 'injected' function calls do not take variable inputs (they use the current queue's variables),
    // and do not output detemrinations!
    // TODO: Explain more!
    // @Example
    // // This example calls the function 'helloworld'.
    // call helloworld
    // @Example
    // // This example calls the function 'outputme' with variable 'text' set to 'hello world'.
    // call outputme "text:hello world"
    // @Example
    // TODO: More examples!
    // @Tags
    // <{var[call_determinations]}> returns what the called function determined, if anything (Only for 'run' mode).
    // @BlockVars
    // call_determinations ListTag
    // -->
    class CallCommand: AbstractCommand
    {
        public CallCommand()
        {
            Name = "call";
            Arguments = "[inject/run] <function to call> [<variable>:<value> ...]";
            Description = "Runs a function.";
            IsFlow = true;
            Asyncable = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(entry);
            }
            else
            {
                string type = entry.GetArgument(0).ToLower();
                bool run = false;
                if (type == "run")
                {
                    run = true;
                }
                else if (type == "inject")
                {
                    run = false;
                }
                else
                {
                    ShowUsage(entry);
                    return;
                }
                string fname = entry.GetArgument(1);
                if (fname == "\0CALLBACK")
                {
                    return;
                }
                fname = fname.ToLower();
                CommandScript script = entry.Queue.CommandSystem.GetFunction(fname);
                if (script != null)
                {
                    entry.Good("Calling '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>' (" + (run ? "run": "inject") + ")...");
                    List<CommandEntry> block = script.GetEntries();
                    block.Add(new CommandEntry("call \0CALLBACK", null, entry,
                            this, new List<Argument> { CommandSystem.TagSystem.SplitToArgument("\0CALLBACK", true) }, "call", 0, entry.ScriptName, entry.ScriptLine));
                    if (run)
                    {
                        CommandQueue queue;
                        Dictionary<string, TemplateObject> variables = new Dictionary<string, TemplateObject>();
                        for (int i = 2; i < entry.Arguments.Count; i++)
                        {
                            string str = entry.GetArgument(i);
                            if (!str.Contains(':'))
                            {
                                entry.Error("Invalid variable input!");
                                return;
                            }
                            string[] split = str.Split(new char[] { ':' }, 2);
                            variables.Add(split[0], new TextTag(split[1]));
                        }
                        entry.Queue.CommandSystem.ExecuteScript(script, variables, out queue);
                        if (!queue.Running)
                        {
                            entry.Finished = true;
                        }
                        else
                        {
                            EntryFinisher fin = new EntryFinisher() { Entry = entry };
                            queue.Complete += new EventHandler<CommandQueueEventArgs>(fin.Complete);
                        }
                        ListTag list = new ListTag(queue.Determinations);
                        entry.Queue.SetVariable("call_determinations", list);
                    }
                    else
                    {
                        entry.Queue.AddCommandsNow(block);
                    }
                }
                else
                {
                    entry.Error("Cannot call function '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>': it does not exist!");
                }
            }
        }
    }
}
