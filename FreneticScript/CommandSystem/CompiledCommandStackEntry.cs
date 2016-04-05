using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represetns a precompiled command stack entry.
    /// </summary>
    public class CompiledCommandStackEntry: CommandStackEntry
    {
        /// <summary>
        /// The compiled object.
        /// </summary>
        public CompiledCommandRunnable[] EntryCommands;

        /// <summary>
        /// Run this command stack.
        /// </summary>
        /// <param name="queue">The queue to run under.</param>
        /// <returns>Whether to continue looping.</returns>
        public override CommandStackRetVal Run(CommandQueue queue)
        {
            while (Index >= 0 && Index < Entries.Length)
            {
                CommandEntry CurrentCommand = Entries[Index];
                CompiledCommandRunnable Runnable = EntryCommands[Index];
                Index++;
                if (CurrentCommand.Command.Waitable && CurrentCommand.WaitFor)
                {
                    queue.WaitingOn = CurrentCommand;
                }
                try
                {
                    Runnable.Run(queue);
                }
                catch (Exception ex)
                {
                    if (!(ex is ErrorInducedException))
                    {
                        try
                        {
                            queue.HandleError(CurrentCommand, "Internal exception: " + ex.ToString());
                        }
                        catch (Exception ex2)
                        {
                            string message = ex2.ToString();
                            if (Debug <= DebugMode.MINIMAL)
                            {
                                queue.CommandSystem.Output.Bad(message, DebugMode.MINIMAL);
                                if (queue.Outputsystem != null)
                                {
                                    queue.Outputsystem.Invoke(message, MessageType.BAD);
                                }
                                Index = Entries.Length + 1;
                                queue.CommandStack.Clear();
                            }
                        }
                    }
                }
                if (queue.Delayable && ((queue.Wait > 0f) || queue.WaitingOn != null))
                {
                    return CommandStackRetVal.BREAK;
                }
                if (queue.CommandStack.Count == 0)
                {
                    return CommandStackRetVal.BREAK;
                }
                if (queue.CommandStack.Peek() != this)
                {
                    return CommandStackRetVal.CONTINUE;
                }
            }
            if (queue.CommandStack.Count > 0)
            {
                queue.CommandStack.Pop();
                if (queue.CommandStack.Count > 0 && Determinations != null)
                {
                    queue.LastDeterminations = Determinations;
                    CommandStackEntry tcse = queue.CommandStack.Peek();
                    tcse.Variables["determinations"] = new ListTag(Determinations);
                }
                else
                {
                    queue.LastDeterminations = null;
                }
                return CommandStackRetVal.CONTINUE;
            }
            return CommandStackRetVal.STOP;
        }
    }
}
