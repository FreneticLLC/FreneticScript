using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem.QueueCmds
{
    class FireeventCommand : AbstractCommand
    {
        public FireeventCommand()
        {
            Name = "fireevent";
            Arguments = "[name of event] (context_name|context_value|...)";
            Description = "Fires an event.";
        }

        public override void Execute(CommandEntry entry)
        {
            // TODO
        }
    }
}
