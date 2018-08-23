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
            if (entry.Arguments.Count < 1)
            {
                entry.Info(queue, "Listing <{text_color[emphasis]}>" + queue.CommandSystem.Context.CVarSys.CVars.Count + "<{text_color[base]}> CVars...");
                for (int i = 0; i < queue.CommandSystem.Context.CVarSys.CVars.Count; i++)
                {
                    CVar cvar = queue.CommandSystem.Context.CVarSys.CVarList[i];
                    entry.Info(queue, "<{text_color[emphasis]}>" + (i + 1).ToString() + "<{text_color[simple]}>)<{text_color[emphasis]}> " + TagParser.Escape(cvar.Info()));
                }
            }
            else
            {
                string target = entry.GetArgument(queue, 0).ToLowerFastFS();
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
                    queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(target) + "<{text_color[base]}>' does not exist!");
                }
                else
                {
                    entry.Info(queue, "Listing <{text_color[emphasis]}>" + cvars.Count + "<{text_color[base]}> CVars...");
                    for (int i = 0; i < cvars.Count; i++)
                    {
                        CVar cvar = cvars[i];
                        entry.Info(queue, "<{text_color[emphasis]}>" + (i + 1).ToString() + "<{text_color[simple]}>)<{text_color[emphasis]}> " + TagParser.Escape(cvar.Info()));
                    }
                }
            }
        }
    }
}
