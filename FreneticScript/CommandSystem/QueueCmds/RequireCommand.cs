using System;
using System.Collections.Generic;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class RequireCommand : AbstractCommand
    {
        // TODO: Meta!
        public RequireCommand()
        {
            Name = "require";
            Arguments = "'loud'/'quiet'/'error' <variable to require> [...]";
            Description = "Stops a command queue entirely or throws an error if the relevant variables are not available.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 2;
            MaximumArguments = -1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                verify,
                TextTag.For
            };
        }

        TemplateObject verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "loud" || inp == "quiet" || inp == "error")
            {
                return new TextTag(inp);
            }
            return null;
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string loud = entry.GetArgument(queue, 0).ToLowerFast();
            for (int i = 1; i < entry.Arguments.Count; i++)
            {
                string arg = entry.GetArgument(queue, i).ToLowerFast();
                CommandStackEntry cse = queue.CommandStack.Peek();
                // TODO: Restore this command. Use compilation!
                /*
                if (!cse.Variables.ContainsKey(arg))
                {
                    if (loud == "loud")
                    {
                        entry.Bad(queue, "Missing variable '" + TagParser.Escape(arg) + "'!");
                        queue.Stop();
                    }
                    else if (loud == "quiet")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Missing variable '" + TagParser.Escape(arg) + "'!");
                        }
                        queue.Stop();
                    }
                    else
                    {
                        queue.HandleError(entry, "Missing variable '" + TagParser.Escape(arg) + "'!");
                    }
                    return;
                }*/
            }
            entry.Good(queue, "Require command passed, all variables present!");
        }
    }
}
