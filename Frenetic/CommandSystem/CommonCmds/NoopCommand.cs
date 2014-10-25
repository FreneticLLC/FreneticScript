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
        }

        public override void Execute(CommandEntry entry)
        {
        }
    }
}
