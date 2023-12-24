//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds;

/// <summary>The Echo command: outputs text to the console.</summary>
[CommandMeta(
    Name = "echo",
    Arguments = "<text to echo>",
    Short = "Echoes any input text back to the console.",
    Updated = "2022/03/02",
    Group = "Common",
    MinimumArgs = 1, MaximumArgs = 1,
    Description = "Whatever text you input, gets output right back to the console, exactly as-is.\n"
                + "Generally useful for debugging or as very quick-n-dirty information outputs for scripts.\n",
    Examples = [ "// This example says hello to the world.\n"
                    + "echo \"Hello world!\";",
                      "// This example tells you a random number.\n"
                    + "echo \"Your number is... <system.random_decimal>\";"]
    )]
public class EchoCommand : AbstractCommand
{
    /// <summary>Constructs the echo command.</summary>
    public EchoCommand()
    {
        ObjectTypes =
        [
            TextTag.Validator
        ];
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        string args = entry.GetArgument(queue, 0);
        CommandEntry.InfoOutput(queue, args);
    }
}
