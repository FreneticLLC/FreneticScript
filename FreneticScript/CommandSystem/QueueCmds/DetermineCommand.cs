using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class DetermineCommand: AbstractCommand
    {
        public DetermineCommand()
        {
            Name = "determine";
            Arguments = "[value to set on the queue]";
            Description = "Sets the value determined on the queue. If no argument is specified, sets null.";
            IsFlow = true;
            Asyncable = true;
        }

        public override void Execute(CommandEntry entry)
        {
            string determ = null;
            if (entry.Arguments.Count > 0)
            {
                determ = entry.GetArgument(0);
            }
            entry.Queue.Determinations.Add(determ);
            entry.Good("<{color.info}>Determination of the queue set to '<{color.emphasis}>" + TagParser.Escape(determ) + "<{color.info}>'.");
        }
    }
}
