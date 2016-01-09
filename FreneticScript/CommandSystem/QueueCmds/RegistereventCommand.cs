using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem.QueueCmds
{
    class RegistereventCommand : AbstractCommand
    {
        public RegistereventCommand()
        {
            Name = "registerevent";
            Arguments = "[name of event]";
            Description = "Registers a custom event, to be fired by the fireevent command.";
        }

        public override void Execute(CommandEntry entry)
        {
            // TODO
        }
    }
}
