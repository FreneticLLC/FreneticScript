using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name call
    // @Arguments <function to call>
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
    // -->
    class CallCommand: AbstractCommand
    {
        public CallCommand()
        {
            Name = "call";
            Arguments = "<function to call>";
            Description = "Runs a function.";
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
                string fname = entry.GetArgument(0);
                if (fname == "\0CALLBACK")
                {
                    return;
                }
                fname = fname.ToLower();
                CommandScript script = entry.Queue.CommandSystem.GetFunction(fname);
                if (script != null)
                {
                    entry.Good("Calling '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>'...");
                    List<CommandEntry> block = script.GetEntries();
                    block.Add(new CommandEntry("call \0CALLBACK", null, entry,
                            this, new List<string> { "\0CALLBACK" }, "call", 0));
                    entry.Queue.AddCommandsNow(block);
                }
                else
                {
                    entry.Bad("Cannot call function '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>': it does not exist!");
                }
            }
        }
    }
}
