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
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Debug command.
    /// </summary>
    public class DebugCommand : AbstractCommand
    {
        // <--[command]
        // @Name debug
        // @Arguments 'full'/'minimal'/'none'
        // @Short Changes the debug mode of the current queue.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Group Queue
        // @Minimum 1
        // @Maximum 1
        // @Description
        // Changes the debug mode of the current queue.
        // Can be: full, minimal, or none.
        // Full shows all debug, minimal shows only errors, none hides everything.
        // Note that this technically only applies to the current stack entry, not the entire queue.
        // Meaning, even if you <@link command call>call<@/link> a function in the current queue, debug mode for that function will not affect the debug of the calling function or script.
        // TODO: Explain more!
        // @Example
        // // This example sets the debug mode to full.
        // debug full;
        // @Example
        // // This example sets the debug mode to minimal.
        // debug minimal;
        // -->

        /// <summary>
        /// Constructs the debug command.
        /// </summary>
        public DebugCommand()
        {
            Name = "debug";
            Arguments = "'full'/'minimal'/'none'";
            Description = "Modifies the debug mode of the current queue.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify
            };
        }

        // TODO: Compile this command neatly!

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFastFS();
            if (inp == "full" || inp == "minimal" || inp == "none")
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
            string modechoice = entry.GetArgument(queue, 0);
            CommandStackEntry cse = queue.CurrentEntry;
            switch (modechoice.ToLowerFastFS())
            {
                case "full":
                    cse.Debug = DebugMode.FULL;
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Queue debug mode set to <{text_color[emphasis]}>full<{text_color[base]}>.");
                    }
                    break;
                case "minimal":
                    cse.Debug = DebugMode.MINIMAL;
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Queue debug mode set to <{text_color[emphasis]}>minimal<{text_color[base]}>.");
                    }
                    break;
                case "none":
                    cse.Debug = DebugMode.NONE;
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Queue debug mode set to <{text_color[emphasis]}>none<{text_color[base]}>.");
                    }
                    break;
                default:
                    queue.HandleError(entry, "Unknown debug mode '<{text_color[emphasis]}>" + modechoice + "<{text_color[base]}>'.");
                    break;
            }
        }
    }
}
