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

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>
    /// The Echo command: outputs text to the console.
    /// </summary>
    public class EchoCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the echo command.
        /// </summary>
        public EchoCommand()
        {
            Name = "echo";
            Arguments = "<text to echo>";
            Description = "Echoes any input text back to the console.";
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new Action<ArgumentValidation>[]
            {
                TextTag.Validator
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string args = entry.GetArgument(queue, 0);
            entry.InfoOutput(queue, args);
        }
    }
}
