//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.CommonCmds;

/// <summary>A non-user-invocable command called when no other command exists.</summary>
public class DebugOutputInvalidCommand: AbstractCommand
{
    // Note: Intentionally no meta!

    /// <summary>Constructs the command.</summary>
    public DebugOutputInvalidCommand()
    {
        Name = "\0DebugOutputInvalidCommand";
        Arguments = "<invalid command name>";
        Description = "Reports that a command is invalid, or submits it to a server.";
        IsDebug = true;
        IsFlow = true;
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = -1;
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        string name = entry.Arguments[0].ToString();
        List<string> args = new(entry.Arguments.Length);
        for (int i = 1; i < entry.Arguments.Length; i++)
        {
            args.Add(entry.Arguments[i].ToString());
        }
        queue.Engine.Context.UnknownCommand(queue, name, [.. args]);
    }
}
