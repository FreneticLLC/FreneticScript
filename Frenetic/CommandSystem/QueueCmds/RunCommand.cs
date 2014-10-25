using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.QueueCmds
{
    class RunCommand: AbstractCommand
    {
        public RunCommand()
        {
            Name = "run";
            Arguments = "<script to run>";
            Description = "Runs a script file.";
            // TODO: DEFINITION ARGS
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
                CommandScript script = entry.Queue.CommandSystem.GetScript(fname);
                if (script != null)
                {
                    entry.Good("Running '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>'...");
                    CommandQueue queue;
                    entry.Queue.CommandSystem.ExecuteScript(script, null, out queue);
                }
                else
                {
                    entry.Bad("Cannot run script '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>': file does not exist!");
                }
            }
        }
    }
}
