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
    /// Cvarinfo command: displays information on CVars.
    /// </summary>
    public class CvarinfoCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Construct the CVar Info Command.
        /// </summary>
        public CvarinfoCommand()
        {
            Name = "cvarinfo";
            Arguments = "[CVar to get info on]";
            Description = "Shows information on a specified CVar, or all of them if one isn't specified.";
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Action<ArgumentValidation>>()
            {
                TextTag.Validator
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            // TODO: Remove/replace CVar system (FDS handler)
            /*
            if (entry.Arguments.Count < 1)
            {
                entry.Info(queue, "Listing " + TextStyle.Separate + queue.CommandSystem.Context.CVarSys.CVars.Count + TextStyle.Base + " CVars...");
                for (int i = 0; i < queue.CommandSystem.Context.CVarSys.CVars.Count; i++)
                {
                    CVar cvar = queue.CommandSystem.Context.CVarSys.CVarList[i];
                    entry.Info(queue, "" + TextStyle.Separate + (i + 1).ToString() + "<{text_color[simple]}>)" + TextStyle.Separate + " " + TagParser.Escape(cvar.Info()));
                }
            }
            else
            {
                string target = entry.GetArgument(queue, 0).ToLowerFast();
                List<CVar> cvars = new List<CVar>();
                for (int i = 0; i < queue.CommandSystem.Context.CVarSys.CVars.Count; i++)
                {
                    if (queue.CommandSystem.Context.CVarSys.CVarList[i].Name.StartsWith(target))
                    {
                        cvars.Add(queue.CommandSystem.Context.CVarSys.CVarList[i]);
                    }
                }
                if (cvars.Count == 0)
                {
                    queue.HandleError(entry, "CVar '" + TextStyle.Separate + target + TextStyle.Base + "' does not exist!");
                }
                else
                {
                    entry.Info(queue, "Listing " + TextStyle.Separate + cvars.Count + TextStyle.Base + " CVars...");
                    for (int i = 0; i < cvars.Count; i++)
                    {
                        CVar cvar = cvars[i];
                        entry.Info(queue, "" + TextStyle.Separate + (i + 1).ToString() + "<{text_color[simple]}>)" + TextStyle.Separate + " " + TagParser.Escape(cvar.Info()));
                    }
                }
            }
            */
        }
    }
}
