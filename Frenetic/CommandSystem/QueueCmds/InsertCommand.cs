using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.QueueCmds
{
    class InsertCommand: AbstractCommand
    {
        public InsertCommand()
        {
            Name = "insert";
            Arguments = "<script to insert>";
            Description = "Inserts a script file to the current command queue.";
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
                    entry.Good("Inserting '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>'...");
                    entry.Queue.AddCommandsNow(script.GetEntries());
                }
                else
                {
                    entry.Bad("Cannot insert script '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>': file does not exist!");
                }
            }
        }
    }
}
