//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>
    /// The Toggle command.
    /// </summary>
    public class ToggleCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the toggle command.
        /// </summary>
        public ToggleCommand()
        {
            Name = "toggle";
            Arguments = "<CVar to toggle>";
            Description = "Toggles a CVar between true and false.";
            // TODO: Make asyncable -> Cvar sys lock?
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string target = entry.GetArgument(queue, 0);
            CVar cvar = queue.CommandSystem.Output.CVarSys.Get(target);
            if (cvar == null)
            {
                queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' does not exist!");
                return;
            }
            if (cvar.Flags.HasFlagsFS(CVarFlag.ServerControl))
            {
                queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' cannot be modified, it is server controlled!");
            }
            if (cvar.Flags.HasFlagsFS(CVarFlag.ReadOnly))
            {
                queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' cannot be modified, it is a read-only system variable!");
            }
            else if (cvar.Flags.HasFlagsFS(CVarFlag.InitOnly) && !queue.CommandSystem.Output.Initializing)
            {
                queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' cannot be modified after game initialization.");
            }
            else if (cvar.Flags.HasFlagsFS(CVarFlag.Delayed) && !queue.CommandSystem.Output.Initializing)
            {
                cvar.Set(cvar.ValueB ? "false" : "true");
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "<{text_color[info]}>CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[info]}>' is delayed, and its value will be calculated after the game is reloaded.");
                }
            }
            else
            {
                cvar.Set(cvar.ValueB ? "false" : "true");
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "<{text_color[info]}>CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) +
                        "<{text_color[info]}>' set to '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Value) + "<{text_color[info]}>'.");
                }
            }
        }
    }
}
