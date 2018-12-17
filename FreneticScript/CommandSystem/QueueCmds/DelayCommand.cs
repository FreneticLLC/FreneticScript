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

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Delay command.
    /// </summary>
    public class DelayCommand : AbstractCommand
    {
        /// <summary>
        /// Adjust list of commands that are formed by an inner block.
        /// </summary>
        /// <param name="entry">The producing entry.</param>
        /// <param name="input">The block of commands.</param>
        /// <param name="fblock">The final block to add to the entry.</param>
        public override void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            entry.BlockEnd -= input.Count;
            input.Clear();
            base.AdaptBlockFollowers(entry, input, fblock);
            fblock.Insert(0, new CommandEntry("WAIT " + entry.Arguments[0].ToString(), 0, 0, entry.Command.CommandSystem.RegisteredCommands["wait"],
                entry.Arguments, "wait", 0, entry.ScriptName, entry.ScriptLine, entry.FairTabulation, entry.System));
        }

        // <--[command]
        // @Name delay
        // @Arguments <time in secnds>
        // @Short Delays the contained blocked of commands for the input amount of time.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Block Always
        // @Group Queue
        // @Minimum 1
        // @Maximum 1
        // @Description
        // Delays the contained blocked of commands for the input amount of time.
        // Note that this will not delay the running queue, but rather launch the command block into its own queue.
        // TODO: Explain more!
        // @Example
        // // This example echos "hi", then "bye".
        // delay 1
        // {
        //     echo "bye";
        // }
        // echo "hi";
        // @Example
        // // TODO: More examples!
        // -->
        
        /// <summary>
        /// Constructs the delays command.
        /// </summary>
        public DelayCommand()
        {
            Name = "delay";
            Arguments = "<time in seconds>";
            Description = "Delays the contained block of commands for the input amount of time.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            if (input.ToString() == "\0CALLBACK")
            {
                return input;
            }
            return NumberTag.TryFor(input);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                return;
            }
            if (entry.InnerCommandBlock == null)
            {
                queue.HandleError(entry, "No commands follow!");
                return;
            }
            if (entry.BlockScript != null)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Re-running delay command...");
                }
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "compiling and running delay command...");
                }
                entry.BlockScript = new CommandScript("__delay__command__", entry.InnerCommandBlock, entry.BlockStart);
            }
            CommandQueue nqueue = entry.BlockScript.ToQueue(entry.Command.CommandSystem);
            nqueue.CommandStack.Peek().Debug = queue.CommandStack.Peek().Debug;
            nqueue.Outputsystem = queue.Outputsystem;
            nqueue.Execute();
            CommandStackEntry cse = queue.CurrentStackEntry;
            cse.Index = entry.BlockEnd + 2;
        }
    }
}
