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
        public override bool Run(CommandQueue queue)
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
                if (CurrentCommand.Command == queue.CommandSystem.DebugInvalidCommand)
                {
                    // Last try - perhaps a command was registered after the script was loaded.
                    // TODO: Do we even want this? Command registration should be high-priority auto-run.
                    AbstractCommand cmd;
                    if (queue.CommandSystem.RegisteredCommands.TryGetValue(CurrentCommand.Name.ToLowerFast(), out cmd))
                    {
                        CurrentCommand.Command = cmd;
                    }
                    if (CurrentCommand.Command.Waitable && CurrentCommand.WaitFor)
                    {
                        queue.WaitingOn = CurrentCommand;
                    }
                    CurrentCommand.Command.Execute(queue, CurrentCommand);
                }
                else
                {
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
                }
                if (queue.Delayable && ((queue.Wait > 0f) || queue.WaitingOn != null))
                {
                    return false;
                }
                if (queue.CommandStack.Count == 0)
                {
                    return false;
                }
                if (queue.CommandStack.Peek() != this)
                {
                    return true;
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
                return true;
            }
            return false;
        }
    }
}
