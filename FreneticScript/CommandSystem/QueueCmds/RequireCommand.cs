using System;
using System.Collections.Generic;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    public class RequireCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the require command.
        /// </summary>
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
                Verify,
                TextTag.For
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "loud" || inp == "quiet" || inp == "error")
            {
                return new TextTag(inp);
            }
            return null;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string loud = entry.GetArgument(queue, 0).ToLowerFast();
            for (int i = 1; i < entry.Arguments.Count; i++)
            {
                string arg = entry.GetArgument(queue, i).ToLowerFast();
                CommandStackEntry cse = queue.CurrentEntry;
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
