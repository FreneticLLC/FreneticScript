using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem.CommonCmds
{
    public class DebugOutputInvalidCommand: AbstractCommand
    {
        public DebugOutputInvalidCommand()
        {
            Name = "\0DebugOutputInvalidCommand";
            Arguments = "<invalid command name>";
            Description = "Reports that a command is invalid, or submits it to a server.";
            IsDebug = true;
            IsFlow = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                string name = entry.GetArgument(0);
                List<string> args = new List<string>(entry.Arguments);
                args.RemoveAt(0);
                entry.Output.UnknownCommand(name, args.ToArray());
                if (entry.Queue.Outputsystem != null)
                {
                    entry.Queue.Outputsystem.Invoke(TextStyle.Color_Error + "Unknown command '" +
                    TextStyle.Color_Standout + name + TextStyle.Color_Error + "'.", MessageType.BAD);
                }
            }
        }
    }
}
