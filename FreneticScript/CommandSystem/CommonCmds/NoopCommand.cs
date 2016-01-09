using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;

namespace Frenetic.CommandSystem.CommonCmds
{
    class NoopCommand: AbstractCommand
    {
        public NoopCommand()
        {
            Name = "noop";
            Arguments = "";
            Description = "Does nothing.";
            IsDebug = true;
            Asyncable = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
        }
    }
}
