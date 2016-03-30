﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a set of commands to be run, and related information.
    /// </summary>
    public class CommandQueue
    {
        /// <summary>
        /// The current stack of all command execution data.
        /// </summary>
        public Stack<CommandStackEntry> CommandStack = new Stack<CommandStackEntry>();
        
        /// <summary>
        /// Whether the queue can be delayed (EG, via a WAIT command).
        /// Almost always true.
        /// </summary>
        public bool Delayable = true;

        /// <summary>
        /// How long until the queue may continue.
        /// </summary>
        public float Wait = 0;

        /// <summary>
        /// Whether the queue is running.
        /// </summary>
        public bool Running = false;

        /// <summary>
        /// The command system running this queue.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// The script that was used to build this queue.
        /// </summary>
        public CommandScript Script;

        /// <summary>
        /// Whether commands in the queue will parse tags.
        /// </summary>
        public TagParseMode ParseTags = TagParseMode.ON;

        /// <summary>
        /// What function to invoke when output is generated.
        /// </summary>
        public Commands.OutputFunction Outputsystem = null;

        /// <summary>
        /// Constructs a new CommandQueue - generally kept to the FreneticScript internals.
        /// TODO: IList _commands -> ListQueue?
        /// </summary>
        public CommandQueue(CommandScript _script, IList<CommandEntry> _commands, Commands _system)
        {
            Script = _script;
            CommandSystem = _system;
            PushToStack(_commands, DebugMode.FULL, new Dictionary<string, TemplateObject>());
        }

        /// <summary>
        /// Pushes a list of already-calculated commands to the command stack.
        /// </summary>
        /// <param name="_commands">The commands to push.</param>
        /// <param name="mode">What debug mode to use.</param>
        /// <param name="vars">What variables to use.</param>
        public void PushToStack(IList<CommandEntry> _commands, DebugMode mode, Dictionary<string, TemplateObject> vars)
        {
            CommandEntry[] cmds = new CommandEntry[_commands.Count];
            for (int i = 0; i < _commands.Count; i++)
            {
                cmds[i] = _commands[i].Duplicate(); // TODO: Rather than duplicating the entries, store an array of data holders in the Command-Stack-Entries?
                cmds[i].Queue = this;
                cmds[i].Output = CommandSystem.Output;
            }
            CommandStackEntry cse = new CommandStackEntry();
            cse.Index = 0;
            cse.Entries = cmds;
            cse.Debug = mode;
            cse.Variables = vars;
            CommandStack.Push(cse);
        }

        /// <summary>
        /// Called when the queue is completed.
        /// </summary>
        public EventHandler<CommandQueueEventArgs> Complete;

        /// <summary>
        /// Starts running the command queue.
        /// </summary>
        public void Execute()
        {
            if (Running)
            {
                return;
            }
            Running = true;
            Tick(0f);
            if (Running)
            {
                CommandSystem.Queues.Add(this);
            }
        }

        /// <summary>
        /// Recalculates and advances the command queue.
        /// <param name="Delta">The time passed this tick.</param>
        /// </summary>
        public void Tick(float Delta)
        {
            if (Delayable && WaitingOn != null)
            {
                return;
            }
            if (Delayable && Wait > 0f)
            {
                Wait -= Delta;
                if (Wait > 0f)
                {
                    return;
                }
                Wait = 0f;
            }
            while (CommandStack.Count > 0)
            {
                CommandStackEntry cse = CommandStack.Peek();
                while (cse != null && cse.Index < cse.Entries.Length)
                {
                    CommandEntry CurrentCommand = cse.Entries[cse.Index];
                    cse.Index++;
                    if (CurrentCommand.Command == CommandSystem.DebugInvalidCommand)
                    {
                        // Last try - perhaps a command was registered after the script was loaded.
                        // TODO: Do we even want this? Command registration should be high-priority auto-run.
                        AbstractCommand cmd;
                        if (CommandSystem.RegisteredCommands.TryGetValue(CurrentCommand.Name.ToLowerFast(), out cmd))
                        {
                            CurrentCommand.Command = cmd;
                        }
                    }
                    if (CurrentCommand.Command.Waitable && CurrentCommand.WaitFor)
                    {
                        WaitingOn = CurrentCommand;
                    }
                    try
                    {
                        CurrentCommand.Command.Execute(CurrentCommand);
                    }
                    catch (Exception ex)
                    {
                        if (!(ex is ErrorInducedException))
                        {
                            try
                            {
                                CurrentCommand.Error("Internal exception: " + ex.ToString());
                            }
                            catch (Exception ex2)
                            {
                                string message = ex2.ToString();
                                if (cse.Debug <= DebugMode.MINIMAL)
                                {
                                    CurrentCommand.Output.Bad(message, DebugMode.MINIMAL);
                                    if (Outputsystem != null)
                                    {
                                        Outputsystem.Invoke(message, MessageType.BAD);
                                    }
                                    cse.Index = cse.Entries.Length + 1;
                                    CommandStack.Clear();
                                }
                            }
                        }
                    }
                    if (Delayable && ((Wait > 0f) || WaitingOn != null))
                    {
                        return;
                    }
                    cse = CommandStack.Count > 0 ? CommandStack.Peek(): null;
                }
                if (CommandStack.Count > 0)
                {
                    CommandStackEntry ncse = CommandStack.Pop();
                    if (CommandStack.Count > 0 && ncse.Determinations != null)
                    {
                        LastDeterminations = ncse.Determinations;
                        CommandStackEntry tcse = CommandStack.Peek();
                        tcse.Variables.Add("determinations", new ListTag(ncse.Determinations));
                    }
                    else
                    {
                        LastDeterminations = null;
                    }
                }
            }
            if (Complete != null)
            {
                Complete(this, new CommandQueueEventArgs(this));
            }
            Running = false;
        }

        /// <summary>
        /// The determinations ran on the lowest level of this queue.
        /// </summary>
        public List<TemplateObject> LastDeterminations = null;

        /// <summary>
        /// Whether this Queue is waiting on the last command.
        /// </summary>
        public CommandEntry WaitingOn = null;
        
        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the current command.
        /// </summary>
        /// <param name="entry">The command entry that errored.</param>
        /// <param name="message">The error message.</param>
        public void HandleError(CommandEntry entry, string message)
        {
            WaitingOn = null;
            CommandStackEntry cse = CommandStack.Peek();
            DebugMode dbmode = cse.Debug;
            while (cse != null)
            {
                for (int i = cse.Index; i < cse.Entries.Length; i++)
                {
                    if (GetCommand(i).Command is TryCommand &&
                        GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        entry.Good("Force-exiting try block.");
                        cse.Index = i;
                        return;
                    }
                }
                cse.Index = cse.Entries.Length + 1;
                CommandStack.Pop();
                cse = CommandStack.Count > 0 ? CommandStack.Peek() : null;
            }
            if (dbmode <= DebugMode.MINIMAL)
            {
                entry.Output.Bad(message, DebugMode.MINIMAL);
                if (Outputsystem != null)
                {
                    Outputsystem.Invoke(message, MessageType.BAD);
                }
            }
        }

        /// <summary>
        /// Gets the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command.</param>
        /// <returns>The specified command.</returns>
        public CommandEntry GetCommand(int index)
        {
            return CommandStack.Peek().Entries[index];
        }
        
        /// <summary>
        /// Immediately stops the Command Queue by jumping to the end.
        /// </summary>
        public void Stop()
        {
            CommandStack.Peek().Index = CommandStack.Peek().Entries.Length + 1;
            CommandStack.Clear();
        }

        /// <summary>
        /// Adds or sets a variable for tags in this queue to use.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value to set on the variable.</param>
        public void SetVariable(string name, TemplateObject value)
        {
            CommandStack.Peek().Variables[name.ToLowerFast()] = value;
        }

        /// <summary>
        /// Gets the value of a variable saved on the queue.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The variable's value.</returns>
        public TemplateObject GetVariable(string name)
        {
            string namelow = name.ToLowerFast();
            TemplateObject value;
            if (CommandStack.Peek().Variables.TryGetValue(namelow, out value))
            {
                return value;
            }
            return null;
        }
    }

    /// <summary>
    /// An enumerattion of the possible debug modes a queue can have.
    /// </summary>
    public enum DebugMode : byte
    {
        /// <summary>
        /// Debug everything.
        /// </summary>
        FULL = 1,
        /// <summary>
        /// Only debug errors.
        /// </summary>
        MINIMAL = 2,
        /// <summary>
        /// Debug nothing.
        /// </summary>
        NONE = 3
    }

    /// <summary>
    /// What mode of parsing a Queue uses.
    /// </summary>
    public enum TagParseMode
    {
        /// <summary>
        /// Parsing entirely disabled.
        /// </summary>
        OFF = 0,
        /// <summary>
        /// Parsing enabled in standard tag-syntax mode.
        /// </summary>
        ON = 1
    }
}
