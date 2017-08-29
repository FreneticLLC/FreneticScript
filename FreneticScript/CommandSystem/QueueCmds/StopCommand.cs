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
    // TODO: Meta!

    /// <summary>
    /// The Stop command.
    /// </summary>
    public class StopCommand : AbstractCommand
    {
        /// <summary>
        /// Constructs the stop command.
        /// </summary>
        public StopCommand()
        {
            Name = "stop";
            Arguments = "['all']";
            Description = "Stops the current command queue.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFastFS();
            if (inp == "all")
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
            if (entry.Arguments.Count > 0 && entry.GetArgument(queue, 0).ToLowerFastFS() == "all")
            {
                int qCount = queue.CommandSystem.Queues.Count;
                if (!queue.CommandSystem.Queues.Contains(queue))
                {
                    qCount++;
                }
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Stopping <{text_color[emphasis]}>" + qCount + "<{text_color[base]}> queue" + (qCount == 1 ? "." : "s."));
                }
                foreach (CommandQueue tqueue in queue.CommandSystem.Queues)
                {
                    tqueue.Stop();
                }
                queue.Stop();
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Stopping current queue.");
                }
                queue.Stop();
            }
        }
    }
}
