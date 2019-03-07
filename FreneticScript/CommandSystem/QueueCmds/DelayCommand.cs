//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
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
            fblock.Insert(0, new CommandEntry("WAIT " + entry.Arguments[0].ToString(), 0, 0, entry.Command.Engine.RegisteredCommands["wait"],
                entry.Arguments, "wait", CommandPrefix.NONE, entry.ScriptName, entry.ScriptLine, entry.FairTabulation, entry.System));
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
            ObjectTypes = new List<Action<ArgumentValidation>>()
            {
                NumberTag.Validator
            };
        }

        // TODO: Compile!

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.InnerCommandBlock == null)
            {
                queue.HandleError(entry, "No commands follow!");
                return;
            }
            // TODO: FunctionTag input!
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
                entry.BlockScript = new CommandScript("__delay__command__", "Special", entry.InnerCommandBlock, entry.BlockStart);
            }
            CommandQueue nqueue = entry.BlockScript.ToQueue(entry.Command.Engine);
            nqueue.CommandStack.Peek().Debug = queue.CommandStack.Peek().Debug;
            nqueue.Outputsystem = queue.Outputsystem;
            nqueue.Execute();
            CommandStackEntry cse = queue.CurrentStackEntry;
            cse.Index = entry.BlockEnd + 2;
        }
    }
}
