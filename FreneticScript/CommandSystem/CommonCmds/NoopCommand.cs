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
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>();
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
