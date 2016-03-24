using System;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class AssertCommand : AbstractCommand
    {
        // TODO: Meta!
        public AssertCommand()
        {
            Name = "assert";
            Arguments = "<requirement> <error message>";
            Description = "Throws an error if a requirement is not 'true'.";
            IsFlow = true;
            MinimumArguments = 2;
            MaximumArguments = 2;
        }

        public override void Execute(CommandEntry entry)
        {
            TemplateObject arg1 = entry.GetArgumentObject(0);
            BooleanTag bt = BooleanTag.TryFor(arg1);
            if (bt == null || !bt.Internal)
            {
                entry.Error("Assertion failed: " + entry.GetArgument(1));
                return;
            }
            entry.Good("Require command passed, all variables present!");
        }
    }
}
