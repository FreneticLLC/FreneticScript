using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.CommonCmds
{
    // TODO: Meta!
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
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            GC.Collect();
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Memory cleaned.");
            }
        }
    }
}
