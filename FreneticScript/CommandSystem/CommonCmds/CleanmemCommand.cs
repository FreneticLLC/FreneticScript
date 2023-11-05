//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.CommonCmds;

// TODO: Meta!
/// <summary>A command to interact with the system garbage collector.</summary>
public class CleanmemCommand: AbstractCommand
{
    /// <summary>Constructs the command.</summary>
    public CleanmemCommand()
    {
        Name = "cleanmem";
        Arguments = "";
        Description = "Forces the system Garbage Collector to run, invoking CPU usage to lower RAM usage.";
        Asyncable = true;
    }

    /// <summary>Executes the command.</summary>
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
