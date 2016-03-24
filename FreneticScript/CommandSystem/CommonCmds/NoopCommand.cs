using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class NoopCommand: AbstractCommand
    {
        // TODO: Meta!
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
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
        }
    }
}
