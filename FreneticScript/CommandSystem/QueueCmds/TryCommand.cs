using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class TryCommand : AbstractCommand
    {
        // TODO: Meta!
        // @Braces always
        public TryCommand()
        {
            Name = "try";
            Arguments = "";
            Description = "Executes the following block of commands and exits forcefully if there is an error.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>();
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
                if (entry.ShouldShowGood())
                {
                    entry.Good("Trying block...");
                }
            }
        }
    }
}
