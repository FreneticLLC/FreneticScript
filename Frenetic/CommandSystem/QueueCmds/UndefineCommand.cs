using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.CommonCmds
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
                if (entry.Queue.Variables.ContainsKey(target.ToLower()))
                {
                    entry.Queue.Variables.Remove(target.ToLower());
                    entry.Good("Queue variable '<{text_color.emphasis}>" + TagParser.Escape(target.ToLower()) + "<{text_color.base}>' removed'.");
                }
                else
                {
                    entry.Bad("Unknown queue variable '<{text_color.emphasis}>" + TagParser.Escape(target.ToLower()) + "<{text_color.base}>'.");
                }
            }
        }
    }
}
