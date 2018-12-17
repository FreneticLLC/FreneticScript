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
using FreneticScript.CommandSystem.Arguments;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class WhileCommandData : AbstractCommandEntryData
    {
        public List<Argument> ComparisonArgs;
        public int Index;
    }

    /// <summary>
    /// The While command.
    /// </summary>
    public class WhileCommand : AbstractCommand
    {
        // TODO: Meta!

        // TODO: Compile!

        /// <summary>
        /// Constructs the while command.
        /// </summary>
        public WhileCommand()
        {
            Name = "while";
            Arguments = "'stop'/'next'/<comparisons>";
            Description = "Executes the following block of commands continuously until the argument is false.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = -1;
            IsBreakable = true;
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
            string count = entry.GetArgument(queue, 0);
            if (count == "\0CALLBACK")
            {
                CommandStackEntry cse = queue.CurrentStackEntry;
                WhileCommandData dat = (WhileCommandData)cse.Entries[entry.BlockStart - 1].GetData(queue);
                dat.Index++;
                if (IfCommand.TryIf(queue, entry, new List<Argument>(dat.ComparisonArgs)))
                {
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "While looping...: " + dat.Index);
                    }
                    cse.Index = entry.BlockStart;
                    return;
                }
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "While stopping.");
                }
            }
            else if (count.ToLowerFast() == "stop")
            {
                CommandStackEntry cse = queue.CurrentStackEntry;
                for (int i = 0; i < cse.Entries.Length; i++)
                {
                    if (queue.GetCommand(i).Command is WhileCommand && queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Stopping a while loop.");
                        }
                        cse.Index = i + 2;
                        return;
                    }
                }
                queue.HandleError(entry, "Cannot stop while: not in one!");
            }
            else if (count.ToLowerFast() == "next")
            {
                CommandStackEntry cse = queue.CurrentStackEntry;
                for (int i = cse.Index - 1; i > 0; i--)
                {
                    if (queue.GetCommand(i).Command is WhileCommand && queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Jumping forward in a while loop.");
                        }
                        cse.Index = i + 1;
                        return;
                    }
                }
                queue.HandleError(entry, "Cannot while repeat: not in one!");
            }
            else
            {
                bool success = IfCommand.TryIf(queue, entry, new List<Argument>(entry.Arguments));
                if (!success)
                {
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Not looping.");
                    }
                    CommandStackEntry cse = queue.CurrentStackEntry;
                    cse.Index = entry.BlockEnd + 2;
                    return;
                }
                entry.SetData(queue, new WhileCommandData() { Index = 1, ComparisonArgs = entry.Arguments });
            }
        }
    }
}
