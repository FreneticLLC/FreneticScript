//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

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
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            GC.Collect();
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Memory cleaned.");
            }
        }
    }
}
