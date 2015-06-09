using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;

namespace Frenetic.CommandSystem.QueueCmds
{
    class GotoCommand : AbstractCommand
    {
        public GotoCommand()
        {
            Name = "goto";
            Arguments = "<mark name>";
            Description = "Goes forward to the next marked location in the script.";
            IsFlow = true;
            Asyncable = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string targ = entry.GetArgument(0);
            bool hasnext = false;
            for (int i = 0; i < entry.Queue.CommandList.Count; i++)
            {
                if (entry.Queue.GetCommand(i).Command is MarkCommand
                    && entry.Queue.GetCommand(i).Arguments[0] == targ)
                {
                    hasnext = true;
                    break;
                }
            }
            if (hasnext)
            {
                entry.Good("Going to mark.");
                while (entry.Queue.CommandList.Count > 0)
                {
                    if (entry.Queue.GetCommand(0).Command is MarkCommand
                        && entry.Queue.GetCommand(0).Arguments[0] == targ)
                    {
                        entry.Queue.RemoveCommand(0);
                        break;
                    }
                    entry.Queue.RemoveCommand(0);
                }
            }
            else
            {
                entry.Bad("Cannot goto marked location: unknown marker!");
            }
        }
    }
}
