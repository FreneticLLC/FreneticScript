﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class GotoCommand : AbstractCommand
    {
        // TODO: Meta!
        public GotoCommand()
        {
            Name = "goto";
            Arguments = "<mark name>";
            Description = "Goes forward to the next marked location in the script.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            string targ = entry.GetArgument(0);
            bool hasnext = false;
            for (int i = 0; i < entry.Queue.CommandList.Length; i++)
            {
                if (entry.Queue.GetCommand(i).Command is MarkCommand
                    && entry.Queue.GetCommand(i).Arguments[0].ToString() == targ)
                {
                    hasnext = true;
                    break;
                }
            }
            if (hasnext)
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Going to mark.");
                }
                while (entry.Queue.CommandList.Length > 0)
                {
                    if (entry.Queue.GetCommand(0).Command is MarkCommand
                        && entry.Queue.GetCommand(0).Arguments[0].ToString() == targ)
                    {
                        entry.Queue.RemoveCommand(0);
                        break;
                    }
                    entry.Queue.RemoveCommand(0);
                }
            }
            else
            {
                entry.Error("Cannot goto marked location: unknown marker!");
            }
        }
    }
}
