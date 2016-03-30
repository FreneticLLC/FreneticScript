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
        // TODO: Meta!

        public override void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            entry.BlockEnd -= input.Count;
            input.Clear();
            base.AdaptBlockFollowers(entry, input, fblock);
            fblock.Insert(0, new CommandEntry("WAIT " + entry.Arguments[0].ToString(), 0, 0, entry.Command.CommandSystem.RegisteredCommands["wait"],
                entry.Arguments, "wait", 0, entry.ScriptName, entry.ScriptLine, entry.FairTabulation));
        }

        public DelayCommand()
        {
            Name = "delay";
            Arguments = "<time in seconds>";
            Description = "Delays the contained block of commands for the input amount of time.";
            IsFlow = true;
            Asyncable = true;
            Waitable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    if (input.ToString() == "\0CALLBACK")
                    {
                        return input;
                    }
                    return NumberTag.TryFor(input);
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                return;
            }
            if (entry.InnerCommandBlock == null)
            {
                entry.Error("No commands follow!");
                return;
            }
            // TODO: Don't regenerate constantly!
            CommandScript script = new CommandScript("__delay__command__", entry.InnerCommandBlock, entry.BlockStart);
            CommandQueue queue = script.ToQueue(entry.Command.CommandSystem);
            queue.CommandStack.Peek().Debug = entry.Queue.CommandStack.Peek().Debug;
            queue.Outputsystem = entry.Queue.Outputsystem;
            queue.Execute();
            if (entry.WaitFor && entry.Queue.WaitingOn == entry)
            {
                entry.Queue.WaitingOn = null;
            }
            CommandStackEntry cse = entry.Queue.CommandStack.Peek();
            cse.Index = entry.BlockEnd + 2;
        }
    }
}
