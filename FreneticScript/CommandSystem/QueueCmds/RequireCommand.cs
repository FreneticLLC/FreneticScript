using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class RequireCommand : AbstractCommand // TODO: Public!
    {
        // TODO: Meta!
        public RequireCommand()
        {
            Name = "require";
            Arguments = "Loud/Quiet/Error <variable to require> [...]";
            Description = "Stops a command queue entirely or throws an error if the relevant variables are not available.";
            IsFlow = true;
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(entry);
                return;
            }
            string loud = entry.GetArgument(0).ToLowerInvariant();
            for (int i = 1; i < entry.Arguments.Count; i++)
            {
                string arg = entry.GetArgument(i);
                if (!entry.Queue.Variables.ContainsKey(arg))
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
