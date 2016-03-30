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
                (input) =>
                {
                    string inp = input.ToString().ToLowerFast();
                    if (inp == "loud" || inp == "quiet" || inp == "error")
                    {
                        return new TextTag(inp);
                    }
                    return null;
                },
                (input) =>
                {
                    return new TextTag(input.ToString());
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            string loud = entry.GetArgument(0).ToLowerFast();
            for (int i = 1; i < entry.Arguments.Count; i++)
            {
                string arg = entry.GetArgument(i).ToLowerFast();
                CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                if (!cse.Variables.ContainsKey(arg))
                {
                    if (loud == "loud")
                    {
                        entry.Bad("Missing variable '" + TagParser.Escape(arg) + "'!");
                        entry.Queue.Stop();
                    }
                    else if (loud == "quiet")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Missing variable '" + TagParser.Escape(arg) + "'!");
                        }
                        entry.Queue.Stop();
                    }
                    else
                    {
                        entry.Error("Missing variable '" + TagParser.Escape(arg) + "'!");
                    }
                    return;
                }
            }
            entry.Good("Require command passed, all variables present!");
        }
    }
}
