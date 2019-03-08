//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using System.Reflection;
using System.Threading;

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
        public Stack<CompiledCommandRunnable> RunningStack = new Stack<CompiledCommandRunnable>();

        /// <summary>
        /// Represents the <see cref="CommandQueue.CurrentRunnable"/> field.
        /// </summary>
        public static FieldInfo COMMANDQUEUE_CURRENTRUNNABLE = typeof(CommandQueue).GetField(nameof(CurrentRunnable));

        /// <summary>
        /// Represents the <see cref="GetTagData"/> method.
        /// </summary>
        public static MethodInfo COMMANDQUEUE_GETTAGDATA = typeof(CommandQueue).GetMethod(nameof(GetTagData));

        /// <summary>
        /// The current command runnable being processed.
        /// </summary>
        public CompiledCommandRunnable CurrentRunnable;
        
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
        public ScriptEngine Engine;

        /// <summary>
        /// The script that was used to build this queue.
        /// </summary>
        public CommandScript Script;
        
        /// <summary>
        /// What function to invoke when output is generated.
        /// </summary>
        public ScriptEngine.OutputFunction Outputsystem = null;

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
                BasicTagData.TagSystem = Engine.TagSystem;
                BasicTagData.ErrorHandler = Error;
            }
            BasicTagData.Runnable = CurrentRunnable;
            BasicTagData.DBMode = CurrentRunnable == null ? DebugMode.FULL : CurrentRunnable.Debug;
            return BasicTagData;
        }

        /// <summary>
        /// Constructs a new CommandQueue - generally kept to the FreneticScript internals.
        /// </summary>
        public CommandQueue(CommandScript _script, ScriptEngine _system)
        {
            Script = _script;
            Engine = _system;
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
            if (CurrentRunnable == null)
            {
                entry = null;
                return false;
            }
            if (CurrentRunnable.Index > CurrentRunnable.Entry.Entries.Length)
            {
                entry = CurrentRunnable.Entry.At(CurrentRunnable.Entry.Entries.Length - 1);
            }
            else
            {
                entry = CurrentRunnable.Entry.At(CurrentRunnable.Index - 1);
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
            entry = CurrentRunnable?.CurrentCommandEntry;
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
            ID = Interlocked.Increment(ref HighestID);
            CurrentRunnable = RunningStack.Peek();
            if (ShouldOutputCurrent(out CommandEntry first))
            {
                first.GoodOutput(this, "Queue " + TextStyle.Separate + ID + TextStyle.Outgood + " started.");
            }
            Tick(0f);
            if (Running)
            {
                Engine.Queues.Add(this);
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
                    current.GoodOutput(this, "Queue " + TextStyle.Separate + ID + TextStyle.Outgood + " resuming processing.");
                }
            }
            while (RunningStack.Count > 0)
            {
                CurrentRunnable = RunningStack.Peek();
                CommandStackRetVal ret = CurrentRunnable.Entry.Run(this, CurrentRunnable);
                if (ret == CommandStackRetVal.BREAK)
                {
                    if (ShouldOutputLast(out CommandEntry current))
                    {
                        current.GoodOutput(this, "Queue " + TextStyle.Separate + ID + TextStyle.Outgood + " waiting.");
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
                last.GoodOutput(this, "Queue " + TextStyle.Separate + ID + TextStyle.Outgood + " completed.");
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
            HandleError(null, message);
        }

        /// <summary>
        /// Handles an error as appropriate to the situation, in the current queue, from the specified command.
        /// </summary>
        /// <param name="entry">The command entry that errored.</param>
        /// <param name="message">The error message.</param>
        public void HandleError(CommandEntry entry, string message)
        {
            if (entry == null)
            {
                entry = CurrentRunnable.Entry.Entries[CurrentRunnable.Index];
            }
            CurrentRunnable.Entry.HandleError(this, entry, message);
        }

        /// <summary>
        /// Parse an argument within this queue.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The object result.</returns>
        public TemplateObject ParseArgument(Argument arg)
        {
            return arg.Parse(Error, CurrentRunnable);
        }

        /// <summary>
        /// Gets the command at the specified index.
        /// </summary>
        /// <param name="index">The index of the command.</param>
        /// <returns>The specified command.</returns>
        public CommandEntry GetCommand(int index)
        {
            return CurrentRunnable.Entry.Entries[index];
        }
        
        /// <summary>
        /// Returns whether commands should output 'good' results.
        /// </summary>
        /// <returns>Whether commands should output 'good' results.</returns>
        public bool ShouldShowGood()
        {
            return CurrentRunnable.Debug == DebugMode.FULL;
        }
        
        /// <summary>
        /// Used to output a success message.
        /// </summary>
        /// <param name="text">The text to output.</param>
        public void GoodOutput(string text)
        {
            if (ShouldShowGood())
            {
                Engine.Context.GoodOutput(text);
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
            CurrentRunnable.Index = CurrentRunnable.Entry.Entries.Length + 1;
            RunningStack.Clear();
        }
        
        /// <summary>
        /// Sets a compiled stack entry's local variable.
        /// </summary>
        /// <param name="c">The location.</param>
        /// <param name="value">The new value.</param>
        public void SetLocalVar(int c, TemplateObject value)
        {
            CurrentRunnable.LocalVariables[c].Internal = value;
        }
    }
}
