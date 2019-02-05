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
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>
    /// The Set command.
    /// </summary>
    public class SetCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the set command.
        /// </summary>
        public SetCommand()
        {
            Name = "set";
            Arguments = "<CVar to set> <new value> ['force'/'remove'/'do_not_save']";
            Description = "Modifies the value of a specified CVar, or creates a new one.";
            // TODO: make asyncable? Probably with a CVar system lock?
            MinimumArguments = 2;
            MaximumArguments = 3;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For,
                TextTag.For,
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "force" || inp == "remove" || inp == "do_not_save")
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
            string target = entry.GetArgument(queue, 0);
            string newvalue = entry.GetArgument(queue, 1);
            string a2 = entry.Arguments.Count > 2 ? entry.GetArgument(queue, 2).ToLowerFast() : "";
            bool force = a2 == "force";
            bool remove = a2 == "remove";
            bool do_not_save = a2 == "do_not_save";
            if (remove)
            {
                CVar Cvar = queue.Engine.Context.CVarSys.Get(target);
                if (Cvar == null)
                {
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.GoodOutput(queue, "CVar '" + TextStyle.Separate + target + TextStyle.Base + "' cannot be removed, it doesn't exist!");
                    }
                }
                else if (!Cvar.Flags.HasFlagsFS(CVarFlag.UserMade))
                {
                    queue.HandleError(entry, "CVar '" + TextStyle.Separate + Cvar.Name + TextStyle.Base + "' cannot be removed, it wasn't user made!");
                }
                else
                {
                    Cvar.Set("");
                    queue.Engine.Context.CVarSys.CVars.Remove(Cvar.Name);
                    queue.Engine.Context.CVarSys.CVarList.Remove(Cvar);
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.GoodOutput(queue, TextStyle.Importantinfo + "CVar '" + TextStyle.Separate + Cvar.Name + TextStyle.Importantinfo + "' removed.");
                    }
                }
                return;
            }
            CVar cvar = queue.Engine.Context.CVarSys.AbsoluteSet(target, newvalue, force, do_not_save ? CVarFlag.DoNotSave : CVarFlag.None);
            if (cvar.Flags.HasFlagsFS(CVarFlag.ServerControl) && !force)
            {
                queue.HandleError(entry, "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Base + "' cannot be modified, it is server controlled!");
                return;
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
                entry.GoodOutput(queue, "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Base + "' is delayed, and its value will be calculated after the game is reloaded.");
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "CVar '" + TextStyle.Separate + cvar.Name + TextStyle.Base + "' set to '" + TextStyle.Separate + cvar.Value + TextStyle.Base + "'.");
                }
            }
        }
    }
}
