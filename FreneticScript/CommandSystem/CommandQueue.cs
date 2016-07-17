using System;
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
        /// The current stack entry being used.
        /// </summary>
        public CommandStackEntry CurrentEntry;
        
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
        /// </summary>
        public CommandQueue(CommandScript _script, Commands _system)
        {
            Script = _script;
            CommandSystem = _system;
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
            if (CommandStack.Count > 0)
            {
                LowestVariables = CommandStack.Peek().Variables;
            }
            Tick(0f);
            if (Running)
            {
                CommandSystem.Queues.Add(this);
            }
        }

        /// <summary>
        /// Recalculates and advances the command queue.
        /// <param name="Delta">The time that passed this tick.</param>
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
                CurrentEntry = CommandStack.Peek();
                CommandStackRetVal ret = CurrentEntry.Run(this);
                if (ret == CommandStackRetVal.BREAK)
                {
                    return;
                }
                else if (ret == CommandStackRetVal.STOP)
                {
                    break;
                }
            }
            if (Complete != null)
            {
                Complete(this, new CommandQueueEventArgs(this));
            }
            Running = false;
        }
        
        /// <summary>
        /// The variables that ran on the lowest level of this queue.
        /// </summary>
        public Dictionary<string, TemplateObject> LowestVariables = null;

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
            CurrentEntry.HandleError(this, entry, message);
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

        /// <summary>
        /// Sets a compiled stack entry's local variable.
        /// </summary>
        /// <param name="c">The location.</param>
        /// <param name="value">The new value.</param>
        public void SetLocalVar(int c, TemplateObject value)
        {
            CompiledCommandStackEntry cse = CommandStack.Peek() as CompiledCommandStackEntry;
            cse.LocalVariables[c] = value;
            cse.Variables[cse.LocalVarNames[c]] = value; // TODO: Remove need for this!
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
