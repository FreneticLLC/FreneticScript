//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.CommonCmds;

// TODO: Meta!
/// <summary>A command to interact with the system garbage collector.</summary>
[CommandMeta(
    Name = "cleanmem",
    Arguments = "[mode]",
    Short = "Forces the system Garbage Collector to run, invoking CPU usage to lower RAM usage.",
    Updated = "2025/05/07",
    Group = "Common",
    Asyncable = true,
    MinimumArgs = 0, MaximumArgs = 1,
    Description = "The optional argument can be 'full' for aggressive full clean, or 'simple' (the default) for less aggressive clean, or 'gentle' for very minimal.",
    Examples = [ "// Does an aggressive full clean immediately. May lag.\n"
                    + "cleanmum full;"]
    )]
public class CleanmemCommand: AbstractCommand
{
    /// <summary>Set of strings allowed as the first arg.</summary>
    public static HashSet<string> Modes = ["full", "simple", "gentle"];

    /// <summary>Constructs the command.</summary>
    public CleanmemCommand()
    {
        static void validator(ArgumentValidation validator)
        {
            if (!Modes.Contains(validator.ObjectValue.ToString()))
            {
                validator.ErrorAction($"Invalid memory cleaning mode '{validator.ObjectValue}': must be one of [{Modes.JoinString(", ")}].");
                return;
            }
            validator.ObjectValue = TextTag.For(validator.ObjectValue);
        }
        ObjectTypes =
        [
            validator
        ];
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        string mode = entry.Arguments.Length == 0 ? "simple" : entry.GetArgument(queue, 0).ToLowerFast();
        if (mode == "full")
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        }
        else if (mode == "simple")
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);
        }
        else if (mode == "gentle")
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, false);
        }
        else
        {
            queue.HandleError(entry, $"Invalid memory cleaning mode '{mode}': must be one of [{Modes.JoinString(", ")}].");
            return;
        }
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "Memory cleaned.");
        }
    }
}
