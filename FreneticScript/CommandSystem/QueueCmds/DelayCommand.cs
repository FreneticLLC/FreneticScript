using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class DelayCommand : AbstractCommand
    {
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
                verify
            };
        }

        TemplateObject verify(TemplateObject input)
        {
            if (input.ToString() == "\0CALLBACK")
            {
                return input;
            }
            return NumberTag.TryFor(input);
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
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
                    entry.Good(queue, "Re-running delay command...");
                }
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "compiling and running delay command...");
                }
                entry.BlockScript = new CommandScript("__delay__command__", entry.InnerCommandBlock, entry.BlockStart, true);
            }
            CommandQueue nqueue = entry.BlockScript.ToQueue(entry.Command.CommandSystem);
            nqueue.CommandStack.Peek().Debug = queue.CommandStack.Peek().Debug;
            nqueue.Outputsystem = queue.Outputsystem;
            nqueue.Execute();
            CommandStackEntry cse = queue.CommandStack.Peek();
            cse.Index = entry.BlockEnd + 2;
        }
    }
}
