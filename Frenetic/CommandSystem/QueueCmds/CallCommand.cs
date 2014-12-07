using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name call
    // @Arguments [inject/run] <function to call>
    // @Short Runs a function.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Description
    // Activates a function created by the <@link command function>function<@/link> command.
    // TODO: Explain more!
    // @Example
    // // This example calls the function 'helloworld'.
    // call helloworld
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
            Arguments = "[inject/run] <function to call>";
            Description = "Runs a function.";
            IsFlow = true;
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
                    entry.Good("Calling '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>' (" + (run ? "run": "inject") + ")...");
                    List<CommandEntry> block = script.GetEntries();
                    block.Add(new CommandEntry("call \0CALLBACK", null, entry,
                            this, new List<string> { "\0CALLBACK" }, "call", 0));
                    if (run)
                    {
                        CommandQueue queue;
                        entry.Queue.CommandSystem.ExecuteScript(script, null, out queue);
                        if (!queue.Running)
                        {
                            entry.Finished = true;
                        }
                        else
                        {
                            EntryFinisher fin = new EntryFinisher() { Entry = entry };
                            queue.Completefunc = fin.Complete;
                        }
                        ListTag list = new ListTag(queue.Determination);
                        entry.Queue.SetVariable("call_determinations", list);
                    }
                    else
                    {
                        entry.Queue.AddCommandsNow(block);
                    }
                }
                else
                {
                    entry.Bad("Cannot call function '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>': it does not exist!");
                }
            }
        }
    }
}
