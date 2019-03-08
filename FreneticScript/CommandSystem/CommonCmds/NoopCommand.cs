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

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>
    /// The NoOp command: does nothing!
    /// </summary>
    public class NoopCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Construct the noop command.
        /// </summary>
        public NoopCommand()
        {
            Name = "noop";
            Arguments = "";
            Description = "Does nothing.";
            IsDebug = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = -1;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
        }
    }
}
