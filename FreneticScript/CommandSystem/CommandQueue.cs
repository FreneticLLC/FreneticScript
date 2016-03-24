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
        /// All commands in this queue.
        /// TODO: Rewrite to be a static array that we roll through, not a rapidly modified queue :(
        /// </summary>
        public CommandEntry[] CommandList;

        /// <summary>
        /// What command we are currently running.
        /// </summary>
        public int CommandIndex;

        /// <summary>
        /// A list of all variables saved in this queue.
        /// </summary>
        public Dictionary<string, TemplateObject> Variables;

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
        /// How much debug information this queue should show.
        /// </summary>
        public DebugMode Debug;

        /// <summary>
        /// Whether commands in the queue will parse tags.
        /// </summary>
        public bool ParseTags = true;

        /// <summary>
        /// What was returned by the determine command for this queue.
        /// TODO: Template object!
        /// </summary>
        public List<string> Determinations = new List<string>();

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
            Variables = new Dictionary<string, TemplateObject>();
            Debug = DebugMode.FULL;
            for (int i = 0; i < _commands.Count; i++)
            {
                _commands[i].Queue = this;
                _commands[i].Output = CommandSystem.Output;
            }
            CommandList = _commands.ToArray();
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
            if (Delayable && WaitingOn)
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
            while (CommandIndex < CommandList.Length)
            {
                CommandEntry CurrentCommand = CommandList[CommandIndex];
                CommandIndex++;
                int cind = CommandIndex;
                try
                {
                    CommandSystem.ExecuteCommand(CurrentCommand, this);
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
                            if (Debug <= DebugMode.MINIMAL)
                            {
                                CurrentCommand.Output.Bad(message, DebugMode.MINIMAL);
                                if (Outputsystem != null)
                                {
                                    Outputsystem.Invoke(message, MessageType.BAD);
                                }
                                CommandIndex = CommandList.Length + 1;
                            }
                        }
                    }
                }
                if (Delayable && ((Wait > 0f) || WaitingOn))
                {
                    return;
                }
            }
            if (Complete != null)
            {
                Complete(this, new CommandQueueEventArgs(this));
            }
            Running = false;
        }

        /// <summary>
        /// Whether this Queue is waiting on the last command.
        /// </summary>
        public bool WaitingOn = false;
        
        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the current command.
        /// </summary>
        /// <param name="entry">The command entry that errored.</param>
        /// <param name="message">The error message.</param>
        public void HandleError(CommandEntry entry, string message)
        {
            for (int i = 0; i < CommandList.Length; i++)
            {
                if (GetCommand(i).Command is TryCommand &&
                    GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                {
                    entry.Good("Force-exiting try block.");
                    CommandIndex = i;
                    return;
                }
            }
            if (Debug <= DebugMode.MINIMAL)
            {
                entry.Output.Bad(message, DebugMode.MINIMAL);
                if (Outputsystem != null)
                {
                    Outputsystem.Invoke(message, MessageType.BAD);
                }
            }
            CommandIndex = CommandList.Length + 1;
        }

        /// <summary>
        /// Gets the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command.</param>
        /// <returns>The specified command.</returns>
        public CommandEntry GetCommand(int index)
        {
            int x = 0;
            while (CommandList.Length > index + x && CommandList[index + x] == null)
            {
                x++;
            }
            return CommandList[index + x];
        }
        
        /// <summary>
        /// Immediately stops the Command Queue by jumping to the end.
        /// </summary>
        public void Stop()
        {
            CommandIndex = CommandList.Length;
        }

        /// <summary>
        /// Adds or sets a variable for tags in this queue to use.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value to set on the variable.</param>
        public void SetVariable(string name, TemplateObject value)
        {
            string namelow = name.ToLowerInvariant();
            Variables.Remove(namelow);
            Variables.Add(namelow, value);
        }

        /// <summary>
        /// Gets the value of a variable saved on the queue.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The variable's value.</returns>
        public TemplateObject GetVariable(string name)
        {
            string namelow = name.ToLowerInvariant();
            TemplateObject value;
            if (Variables.TryGetValue(namelow, out value))
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
    /// A mini-class used for the callback for &amp;waitable commands.
    /// </summary>
    public class EntryFinisher
    {
        /// <summary>
        /// The entry being waited on.
        /// </summary>
        public CommandEntry Entry;

        /// <summary>
        /// Add this function as a callback to complete entry.
        /// </summary>
        public void Complete(object sender, CommandQueueEventArgs args)
        {
            Entry.Finished = true;
        }
    }
}
