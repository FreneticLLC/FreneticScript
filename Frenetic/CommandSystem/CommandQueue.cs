using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem
{
    /// <summary>
    /// Represents a set of commands to be run, and related information.
    /// </summary>
    public class CommandQueue
    {
        /// <summary>
        /// All commands in this queue, as strings.
        /// </summary>
        public List<CommandEntry> CommandList;

        /// <summary>
        /// A list of all variables saved in this queue.
        /// </summary>
        public Dictionary<string, string> Variables;

        /// <summary>
        /// Whether the queue can be delayed (EG, via a WAIT command).
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
        /// The last command to be run.
        /// </summary>
        public CommandEntry LastCommand;

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
        /// </summary>
        public List<string> Determination = new List<string>();

        public Commands.OutputFunction Outputsystem = null;

        public CommandQueue(CommandScript _script, List<CommandEntry> _commands, Commands _system)
        {
            Script = _script;
            CommandList = _commands;
            CommandSystem = _system;
            Variables = new Dictionary<string, string>();
            Debug = DebugMode.FULL;
        }

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
        /// <param name="Delta">The time passed this tick</param>
        /// </summary>
        public void Tick(float Delta)
        {
            if (Delayable && Wait > 0f)
            {
                Wait -= Delta;
                if (Wait > 0)
                {
                    return;
                }
                Wait = 0;
            }
            while (CommandList.Count > 0)
            {
                CommandEntry CurrentCommand = CommandList[0];
                CommandList.RemoveAt(0);
                CommandSystem.ExecuteCommand(CurrentCommand, this);
                LastCommand = CurrentCommand;
                if (Delayable && Wait > 0f)
                {
                    return;
                }
            }
            Running = false;
        }

        /// <summary>
        /// Gets the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command</param>
        /// <returns>The specified command</returns>
        public CommandEntry GetCommand(int index)
        {
            return CommandList[index];
        }

        /// <summary>
        /// Removes the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command</param>
        public void RemoveCommand(int index)
        {
            CommandList.RemoveAt(index);
        }

        /// <summary>
        /// Adds a list of entries to be executed next in line.
        /// </summary>
        /// <param name="entries">Commands to be run</param>
        public void AddCommandsNow(List<CommandEntry> entries)
        {
            CommandList.InsertRange(0, entries);
        }

        /// <summary>
        /// Immediately stops the Command Queue.
        /// </summary>
        public void Stop()
        {
            CommandList.Clear();
        }

        /// <summary>
        /// Adds or sets a variable for tags in this queue to use.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="value">The value to set on the variable</param>
        public void SetVariable(string name, string value)
        {
            string namelow = name.ToLower();
            Variables.Remove(namelow);
            Variables.Add(namelow, value);
        }

        /// <summary>
        /// Gets the value of a variable saved on the queue.
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>The variable's value</returns>
        public string GetVariable(string name)
        {
            string namelow = name.ToLower();
            string value;
            if (Variables.TryGetValue(namelow, out value))
            {
                return value;
            }
            return null;
        }
    }

    public enum DebugMode : byte
    {
        FULL = 1,
        MINIMAL = 2,
        NONE = 3
    }
}
