//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;
using System.Reflection;

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
        public Stack<CompiledCommandStackEntry> CommandStack = new Stack<CompiledCommandStackEntry>();

        /// <summary>
        /// Represents the <see cref="CommandQueue.CurrentEntry"/> field.
        /// </summary>
        public static FieldInfo COMMANDQUEUE_CURRENTENTRY = typeof(CommandQueue).GetField(nameof(CommandQueue.CurrentEntry));

        /// <summary>
        /// Represents the <see cref="GetTagData"/> method.
        /// </summary>
        public static MethodInfo COMMANDQUEUE_GETTAGDATA = typeof(CommandQueue).GetMethod(nameof(GetTagData));

        /// <summary>
        /// The current stack entry being used.
        /// </summary>
        public CompiledCommandStackEntry CurrentEntry;
        
        /// <summary>
        /// Whether the queue can be delayed (EG, via a WAIT command).
        /// Almost always true.
        /// </summary>
        public bool Delayable = true;

        /// <summary>
        /// How long until the queue may continue.
        /// </summary>
        public double Wait = 0;

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
        /// A basic tag data object for this queue. Not necessarily generated, use <see cref="GetTagData"/>.
        /// </summary>
        public TagData BasicTagData = null;

        /// <summary>
        /// Error handler action.
        /// </summary>
        public Action<string> Error;

        /// <summary>
        /// Gets a basic tag data object appropriate to this queue.
        /// </summary>
        /// <returns>The tag data object.</returns>
        public TagData GetTagData()
        {
            if (BasicTagData == null)
            {
                BasicTagData = TagData.GenerateSimpleErrorTagData();
                BasicTagData.TagSystem = CommandSystem.TagSystem;
                BasicTagData.Error = Error;
            }
            BasicTagData.CSE = CurrentEntry;
            BasicTagData.DBMode = CurrentEntry == null ? DebugMode.FULL : CurrentEntry.Debug;
            return BasicTagData;
        }

        /// <summary>
        /// Constructs a new CommandQueue - generally kept to the FreneticScript internals.
        /// </summary>
        public CommandQueue(CommandScript _script, Commands _system)
        {
            Script = _script;
            CommandSystem = _system;
            Error = HandleError;
        }
        
        /// <summary>
        /// Called when the queue is completed.
        /// </summary>
        public EventHandler<CommandQueueEventArgs> Complete;

        /// <summary>
        /// Current highest ID.
        /// </summary>
        public static long HighestID = 1;

        /// <summary>
        /// This queue's ID.
        /// </summary>
        public long ID;

        /// <summary>
        /// Whether the last queue entry should output.
        /// </summary>
        /// <param name="entry">The last entry.</param>
        /// <returns>Whether it should output.</returns>
        public bool ShouldOutputLast(out CommandEntry entry)
        {
            if (CurrentEntry == null)
            {
                entry = null;
                return false;
            }
            if (CurrentEntry.Index > CurrentEntry.Entries.Length)
            {
                entry = CurrentEntry.At(CurrentEntry.Entries.Length - 1);
            }
            else
            {
                entry = CurrentEntry.At(CurrentEntry.Index - 1);
            }
            return entry != null && entry.CorrectDBMode(this) == DebugMode.FULL;
        }

        /// <summary>
        /// Whether the current queue entry should output.
        /// </summary>
        /// <param name="entry">The current entry.</param>
        /// <returns>Whether it should output.</returns>
        public bool ShouldOutputCurrent(out CommandEntry entry)
        {
            entry = CurrentEntry?.CurrentCommandEntry;
            return entry != null && entry.CorrectDBMode(this) == DebugMode.FULL;
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
            ID = HighestID++; // TODO: thread-safe atomic increment?
            CurrentEntry = CommandStack.Peek();
            if (ShouldOutputCurrent(out CommandEntry first))
            {
                first.GoodOutput(this, "Queue " + TextStyle.Color_Separate + ID + TextStyle.Color_Outgood + " started.");
            }
            Tick(0f);
            if (Running)
            {
                CommandSystem.Queues.Add(this);
            }
        }

        /// <summary>
        /// Whether the last run of this queue had waited.
        /// </summary>
        public bool DidWaitLast = false;

        /// <summary>
        /// Recalculates and advances the command queue.
        /// <param name="Delta">The time that passed this tick.</param>
        /// </summary>
        public void Tick(double Delta)
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
            if (DidWaitLast)
            {
                DidWaitLast = false;
                if (ShouldOutputCurrent(out CommandEntry current))
                {
                    current.GoodOutput(this, "Queue " + TextStyle.Color_Separate + ID + TextStyle.Color_Outgood + " processing.");
                }
            }
            while (CommandStack.Count > 0)
            {
                CurrentEntry = CommandStack.Peek();
                CommandStackRetVal ret = CurrentEntry.Run(this);
                if (ret == CommandStackRetVal.BREAK)
                {
                    if (ShouldOutputLast(out CommandEntry current))
                    {
                        current.GoodOutput(this, "Queue " + TextStyle.Color_Separate + ID + TextStyle.Color_Outgood + " waiting.");
                    }
                    DidWaitLast = true;
                    return;
                }
                else if (ret == CommandStackRetVal.STOP)
                {
                    break;
                }
            }
            Complete?.Invoke(this, new CommandQueueEventArgs(this));
            Running = false;
            if (ShouldOutputLast(out CommandEntry last))
            {
                last.GoodOutput(this, "Queue " + TextStyle.Color_Separate + ID + TextStyle.Color_Outgood + " completed.");
            }
        }

        /// <summary>
        /// Whether this Queue is waiting on the last command.
        /// </summary>
        public CommandEntry WaitingOn = null;

        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the current command.
        /// </summary>
        /// <param name="message">The error message.</param>
        public void HandleError(string message)
        {
            HandleError(CurrentEntry.Entries[CurrentEntry.Index], message);
        }

        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the specified command.
        /// </summary>
        /// <param name="entry">The command entry that errored.</param>
        /// <param name="message">The error message.</param>
        public void HandleError(CommandEntry entry, string message)
        {
            CurrentEntry.HandleError(this, entry, message);
        }

        /// <summary>
        /// Parse an argument within this queue.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The object result.</returns>
        public TemplateObject ParseArgument(Argument arg)
        {
            return arg.Parse(Error, CurrentEntry);
        }

        /// <summary>
        /// Gets the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command.</param>
        /// <returns>The specified command.</returns>
        public CommandEntry GetCommand(int index)
        {
            return CurrentEntry.Entries[index];
        }
        
        /// <summary>
        /// Returns whether commands should output 'good' results.
        /// </summary>
        /// <returns>Whether commands should output 'good' results.</returns>
        public bool ShouldShowGood()
        {
            return CurrentEntry.Debug == DebugMode.FULL;
        }
        
        /// <summary>
        /// Used to output a success message.
        /// </summary>
        /// <param name="text">The text to output.</param>
        public void GoodOutput(string text)
        {
            if (CurrentEntry.Debug == DebugMode.FULL)
            {
                CommandSystem.Context.GoodOutput(text);
                if (Outputsystem != null)
                {
                    Outputsystem.Invoke(text, MessageType.GOOD);
                }
            }
        }

        /// <summary>
        /// Immediately stops the Command Queue by jumping to the end.
        /// </summary>
        public void Stop()
        {
            CurrentEntry.Index = CurrentEntry.Entries.Length + 1;
            CommandStack.Clear();
        }
        
        /// <summary>
        /// Sets a compiled stack entry's local variable.
        /// </summary>
        /// <param name="c">The location.</param>
        /// <param name="value">The new value.</param>
        public void SetLocalVar(int c, TemplateObject value)
        {
            CurrentEntry.LocalVariables[c].Internal = value;
        }
    }

    /// <summary>
    /// Holds an object.
    /// </summary>
    public class ObjectHolder
    {
        /// <summary>
        /// The held object.
        /// </summary>
        public TemplateObject Internal;
    }

    /// <summary>
    /// An enumeration of the possible debug modes a queue can have.
    /// Lower values show more debug output. Higher values show less.
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
