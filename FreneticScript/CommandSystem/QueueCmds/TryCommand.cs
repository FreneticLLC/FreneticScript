using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class TryCommand : AbstractCommand
    {
        public TryCommand()
        {
            Name = "try";
            Arguments = "";
            Description = "Executes the following block of commands and exits forcefully if there is an error.";
            IsFlow = true;
            Asyncable = true;
        }
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.GetArgument(0) == "\0CALLBACK")
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("Block completed successfully!");
                }
            }
            else
            {
                if (entry.Block != null)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Trying block...");
                    }
                    CommandEntry callback = new CommandEntry("try \0CALLBACK", null, entry,
                        this, new List<Argument>() { CommandSystem.TagSystem.SplitToArgument("\0CALLBACK", true) }, "try", 0, entry.ScriptName, entry.ScriptLine);
                    entry.Block.Add(callback);
                    entry.Queue.AddCommandsNow(entry.Block);
                }
                else
                {
                    entry.Error("Try invalid: No block follows!");
                }
            }
        }
    }
}
