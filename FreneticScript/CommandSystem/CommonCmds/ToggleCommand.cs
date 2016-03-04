using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.CommonCmds
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
                    entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' does not exist!");
                    return;
                }
                if (cvar.Flags.HasFlag(CVarFlag.ServerControl))
                {
                    entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' cannot be modified, it is server controlled!");
                }
                if (cvar.Flags.HasFlag(CVarFlag.ReadOnly))
                {
                    entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' cannot be modified, it is a read-only system variable!");
                }
                else if (cvar.Flags.HasFlag(CVarFlag.InitOnly) && !entry.Output.Initializing)
                {
                    entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' cannot be modified after game initialization.");
                }
                else if (cvar.Flags.HasFlag(CVarFlag.Delayed) && !entry.Output.Initializing)
                {
                    cvar.Set(cvar.ValueB ? "false" : "true");
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("<{text_color.info}>CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.info}>' is delayed, and its value will be calculated after the game is reloaded.");
                    }
                }
                else
                {
                    cvar.Set(cvar.ValueB ? "false" : "true");
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("<{text_color.info}>CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) +
                            "<{text_color.info}>' set to '<{text_color.emphasis}>" + TagParser.Escape(cvar.Value) + "<{text_color.info}>'.");
                    }
                }
            }
        }
    }
}
