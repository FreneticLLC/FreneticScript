using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.CommonCmds
{
    class ToggleCommand: AbstractCommand
    {
        public ToggleCommand()
        {
            Name = "toggle";
            Arguments = "<CVar to toggle>";
            Description = "Toggles a CVar between true and false.";
            // TODO: Make asyncable
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
                string target = entry.GetArgument(0);
                CVar cvar = entry.Output.CVarSys.Get(target);
                if (cvar == null)
                {
                    entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name)
                        + "<{color.base}>' does not exist!");
                    return;
                }
                if (cvar.Flags.HasFlag(CVarFlag.ServerControl))
                {
                    entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name)
                        + "<{color.base}>' cannot be modified, it is server controlled!");
                }
                if (cvar.Flags.HasFlag(CVarFlag.ReadOnly))
                {
                    entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name)
                        + "<{color.base}>' cannot be modified, it is a read-only system variable!");
                }
                else if (cvar.Flags.HasFlag(CVarFlag.InitOnly) && !entry.Output.Initializing)
                {
                    entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name)
                        + "<{color.base}>' cannot be modified after game initialization.");
                }
                else if (cvar.Flags.HasFlag(CVarFlag.Delayed) && !entry.Output.Initializing)
                {
                    cvar.Set(cvar.ValueB ? "false" : "true");
                    entry.Good("<{color.info}>CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name) +
                        "<{color.info}>' is delayed, and its value will be calculated after the game is reloaded.");
                }
                else
                {
                    cvar.Set(cvar.ValueB ? "false" : "true");
                    entry.Good("<{color.info}>CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name) +
                        "<{color.info}>' set to '<{color.emphasis}>" + TagParser.Escape(cvar.Value) + "<{color.info}>'.");
                }
            }
        }
    }
}
