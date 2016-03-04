using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class InsertCommand: AbstractCommand
    {
        public InsertCommand() // TODO: Possibly merge with run command?
        {
            Name = "insert";
            Arguments = "<script to insert>";
            Description = "Inserts a script file to the current command queue.";
            IsFlow = true;
            Asyncable = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                // TODO: Events?
                string fname = entry.GetArgument(0);
                CommandScript script = entry.Queue.CommandSystem.GetScript(fname);
                if (script != null)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Inserting '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>'...");
                    }
                    entry.Queue.AddCommandsNow(script.GetEntries());
                }
                else
                {
                    entry.Error("Cannot insert script '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>': file does not exist!");
                }
            }
        }
    }
}
