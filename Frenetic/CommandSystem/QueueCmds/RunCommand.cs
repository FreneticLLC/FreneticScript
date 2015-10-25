using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Frenetic.CommandSystem.CommandEvents;

namespace Frenetic.CommandSystem.QueueCmds
{
    class RunCommand : AbstractCommand
    {
        // TODO: Docs
        // @Waitable
        public RunCommand()
        {
            Name = "run";
            Arguments = "<script to run>";
            Description = "Runs a script file.";
            // TODO: DEFINITION ARGS
            IsFlow = true;
            Waitable = true;
            Asyncable = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                entry.Finished = true;
                return;
            }
            string fname = entry.GetArgument(0).ToLower();
            ScriptRanScriptEvent evt = CommandSystem.ScriptRan.Run(fname);
            if (evt.Cancelled)
            {
                entry.Bad("Script running cancelled via the ScriptRan script event.");
                return;
            }
            CommandScript script = entry.Queue.CommandSystem.GetScript(evt.ScriptName.ToString());
            if (script != null)
            {
                entry.Good("Running '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>'...");
                CommandQueue queue;
                entry.Queue.CommandSystem.ExecuteScript(script, null, out queue);
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
                entry.Queue.SetVariable("run_determinations", list);
            }
            else
            {
                entry.Bad("Cannot run script '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>': file does not exist!");
                entry.Finished = true;
            }
        }
    }
}
