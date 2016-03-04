using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class UndefineCommand: AbstractCommand
    {
        public UndefineCommand()
        {
            Name = "undefine";
            Arguments = "<Variable to remove>";
            Description = "Removes the specified queue variable.";
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
                string target = entry.GetArgument(0);
                if (entry.Queue.Variables.ContainsKey(target.ToLowerInvariant()))
                {
                    entry.Queue.Variables.Remove(target.ToLowerInvariant());
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Queue variable '<{text_color.emphasis}>" + TagParser.Escape(target.ToLowerInvariant()) + "<{text_color.base}>' removed'.");
                    }
                }
                else
                {
                    entry.Error("Unknown queue variable '<{text_color.emphasis}>" + TagParser.Escape(target.ToLowerInvariant()) + "<{text_color.base}>'.");
                }
            }
        }
    }
}
