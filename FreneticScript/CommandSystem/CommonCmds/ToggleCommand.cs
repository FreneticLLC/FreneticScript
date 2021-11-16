//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
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
    /// <summary>The Toggle command.</summary>
    public class ToggleCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>Constructs the toggle command.</summary>
        public ToggleCommand()
        {
            Name = "toggle";
            Arguments = "<CVar to toggle>";
            Description = "Toggles a CVar between true and false.";
            // TODO: Make asyncable -> Cvar sys lock?
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new Action<ArgumentValidation>[]
            {
                TextTag.Validator
            };
        }

        /// <summary>Executes the command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string target = entry.GetArgument(queue, 0);
            CVar cvar = queue.Engine.Context.CVarSys.Get(target);
            if (cvar == null)
            {
                queue.HandleError(entry, "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Base + "' does not exist!");
                return;
            }
            if (cvar.Flags.HasFlagsFS(CVarFlag.ServerControl))
            {
                queue.HandleError(entry, "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Base + "' cannot be modified, it is server controlled!");
            }
            if (cvar.Flags.HasFlagsFS(CVarFlag.ReadOnly))
            {
                queue.HandleError(entry, "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Base + "' cannot be modified, it is a read-only system variable!");
            }
            else if (cvar.Flags.HasFlagsFS(CVarFlag.InitOnly) && !queue.Engine.Context.Initializing)
            {
                queue.HandleError(entry, "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Base + "' cannot be modified after game initialization.");
            }
            else if (cvar.Flags.HasFlagsFS(CVarFlag.Delayed) && !queue.Engine.Context.Initializing)
            {
                cvar.Set(cvar.ValueB ? "false" : "true");
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, TextStyle.Importantinfo + "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Importantinfo + "' is delayed, and its value will be calculated after the game is reloaded.");
                }
            }
            else
            {
                cvar.Set(cvar.ValueB ? "false" : "true");
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, TextStyle.Importantinfo + "CVar '" + TextStyle.Separate + cvar.Name +
                        TextStyle.Importantinfo + "' set to '" + TextStyle.Separate + cvar.Value + TextStyle.Importantinfo + "'.");
                }
            }
        }
    }
}
