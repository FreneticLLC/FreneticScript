using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;

namespace Frenetic.CommandSystem.CommonCmds
{
    class EchoCommand: AbstractCommand
    {
        public EchoCommand()
        {
            Name = "echo";
            Arguments = "<text to echo>";
            Description = "Echoes any input text back to the console.";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                string args = entry.AllArguments();
                entry.Info(TextStyle.Color_Simple + args);
            }
        }
    }
}
