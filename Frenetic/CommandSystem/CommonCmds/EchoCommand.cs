using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.CommonCmds
{
    class EchoCommand: AbstractCommand
    {
        public EchoCommand()
        {
            Name = "echo";
            Arguments = "<text to echo>";
            Description = "Echoes any input text back to the console.";
            Asyncable = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
            }
            else
            {
                string args = entry.AllArguments();
                entry.Info(TextStyle.Color_Simple + TagParser.Escape(args));
            }
        }
    }
}
