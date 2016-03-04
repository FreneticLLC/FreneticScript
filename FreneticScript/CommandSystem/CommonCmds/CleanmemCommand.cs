using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>
    /// A command to interact with the system garbage collector.
    /// </summary>
    public class CleanmemCommand: AbstractCommand
    {
        /// <summary>
        /// Constructs the command.
        /// </summary>
        public CleanmemCommand()
        {
            Name = "cleanmem";
            Arguments = "";
            Description = "Forces the system Garbage Collector to run, invoking CPU usage to lower RAM usage.";
            Asyncable = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            GC.Collect();
            if (entry.ShouldShowGood())
            {
                entry.Good("Memory cleaned.");
            }
        }
    }
}
