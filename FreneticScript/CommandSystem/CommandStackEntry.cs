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
    /// Represents a single entry in a command stack.
    /// </summary>
    public class CommandStackEntry
    {
        /// <summary>
        /// The index of the currently running command.
        /// </summary>
        public int Index;

        /// <summary>
        /// All available commands.
        /// </summary>
        public CommandEntry[] Entries;

        /// <summary>
        /// All entry data available in this CommandStackEntry.
        /// </summary>
        public AbstractCommandEntryData[] EntryData;
        
        /// <summary>
        /// Run this command stack.
        /// </summary>
        /// <param name="queue">The queue to run under.</param>
        /// <returns>Whether to continue looping.</returns>
        public bool Run(CommandQueue queue)
        {
            while (Index < Entries.Length)
            {
                CommandEntry CurrentCommand = Entries[Index];
                Index++;
                if (CurrentCommand.Command == queue.CommandSystem.DebugInvalidCommand)
                {
                    // Last try - perhaps a command was registered after the script was loaded.
                    // TODO: Do we even want this? Command registration should be high-priority auto-run.
                    AbstractCommand cmd;
                    if (queue.CommandSystem.RegisteredCommands.TryGetValue(CurrentCommand.Name.ToLowerFast(), out cmd))
                    {
                        CurrentCommand.Command = cmd;
                    }
                }
                if (CurrentCommand.Command.Waitable && CurrentCommand.WaitFor)
                {
                    queue.WaitingOn = CurrentCommand;
                }
                try
                {
                    CurrentCommand.Command.Execute(queue, CurrentCommand);
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

        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the current command.
        /// </summary>
        /// <param name="queue">The associated queue.</param>
        /// <param name="entry">The command entry that errored.</param>
        /// <param name="message">The error message.</param>
        public void HandleError(CommandQueue queue, CommandEntry entry, string message)
        {
            StringBuilder stacktrace = new StringBuilder();
            stacktrace.Append("ERROR: \"" + message + "\"\n    in script '" + entry.ScriptName + "' at line " + (entry.ScriptLine + 1)
                + ": (" + entry.Name + ")\n");
            queue.WaitingOn = null;
            CommandStackEntry cse = queue.CommandStack.Peek();
            DebugMode dbmode = cse.Debug;
            while (cse != null)
            {
                for (int i = cse.Index; i < cse.Entries.Length; i++)
                {
                    if (queue.GetCommand(i).Command is TryCommand &&
                        queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        entry.Good(queue, "Force-exiting try block.");
                        queue.SetVariable("stack_trace", new TextTag(stacktrace.ToString().Substring(0, stacktrace.Length - 1)));
                        cse.Index = i;
                        throw new ErrorInducedException();
                    }
                }
                cse.Index = cse.Entries.Length + 1;
                queue.CommandStack.Pop();
                if (queue.CommandStack.Count > 0)
                {
                    cse = queue.CommandStack.Peek();
                    if (cse.Index <= cse.Entries.Length)
                    {
                        stacktrace.Append("    in script '" + cse.Entries[cse.Index - 1].ScriptName + "' at line " + (cse.Entries[cse.Index - 1].ScriptLine + 1)
                            + ": (" + cse.Entries[cse.Index - 1].Name + ")\n");
                    }
                }
                else
                {
                    cse = null;
                    break;
                }
            }
            message = stacktrace.ToString().Substring(0, stacktrace.Length - 1);
            if (dbmode <= DebugMode.MINIMAL)
            {
                queue.CommandSystem.Output.Bad(message, DebugMode.MINIMAL);
                if (queue.Outputsystem != null)
                {
                    queue.Outputsystem.Invoke(message, MessageType.BAD);
                }
            }
            throw new ErrorInducedException();
        }

        /// <summary>
        /// All variables available in this portion of the stack.
        /// </summary>
        public Dictionary<string, TemplateObject> Variables;

        /// <summary>
        /// How much debug information this portion of the stack should show.
        /// </summary>
        public DebugMode Debug;

        /// <summary>
        /// What was returned by the determine command for this portion of the stack.
        /// </summary>
        public List<TemplateObject> Determinations = new List<TemplateObject>();
    }
}
