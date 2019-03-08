//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;
using FreneticScript.ScriptSystems;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a precompiled command stack entry.
    /// </summary>
    public class CompiledCommandStackEntry
    {
        /// <summary>
        /// The compiled runner object.
        /// </summary>
        public CompiledCommandRunnable ReferenceCompiledRunnable;

        /// <summary>
        /// Where in the CIL code each entry starts.
        /// </summary>
        public Label[] AdaptedILPoints;

        /// <summary>
        /// The script that sourced this entry.
        /// </summary>
        public CommandScript Script;

        /// <summary>
        /// The generated assembly name.
        /// </summary>
        public String AssemblyName;

        /// <summary>
        /// The backing command system.
        /// </summary>
        public ScriptEngine System
        {
            get
            {
                return Script.System;
            }
        }

        /// <summary>
        /// All available commands.
        /// </summary>
        public CommandEntry[] Entries;

        /// <summary>
        /// The variables on the stack entry.
        /// </summary>
        public SingleCILVariable[] Variables;

        /// <summary>
        /// Gets the command entry at a specified index.
        /// </summary>
        /// <param name="index">The specified index.</param>
        /// <returns>The command entry, or null.</returns>
        public CommandEntry At(int index)
        {
            if (index < 0 || index >= Entries.Length)
            {
                return null;
            }
            return Entries[index];
        }

        /// <summary>
        /// Setters for variables by ID.
        /// </summary>
        public Action<CompiledCommandRunnable, TemplateObject>[] VariableSetters;

        /// <summary>
        /// Gets a setter action, constructed on-demand.
        /// </summary>
        /// <param name="variable">The variable ID.</param>
        /// <returns>The setter.</returns>
        public Action<CompiledCommandRunnable, TemplateObject> GetSetter(int variable)
        {
            if (VariableSetters[variable] == null)
            {
                VariableSetters[variable] = ScriptCompiler.CreateVariableSetter(Variables[variable]);
            }
            return VariableSetters[variable];
        }

        /// <summary>
        /// Run this command stack.
        /// </summary>
        /// <param name="queue">The queue to run under.</param>
        /// <param name="runnable">The runnable to run.</param>
        /// <returns>Whether to continue looping.</returns>
        public CommandStackRetVal Run(CommandQueue queue, CompiledCommandRunnable runnable)
        {
            runnable.CurrentQueue = queue;
            try
            {
                runnable.Run(queue);
                runnable.Index++;
                if (queue.Delayable && ((queue.Wait > 0f) || queue.WaitingOn != null))
                {
                    return CommandStackRetVal.BREAK;
                }
                runnable.Callback?.Invoke();
                if (queue.RunningStack.Count == 0)
                {
                    return CommandStackRetVal.BREAK;
                }
                if (queue.RunningStack.Peek() != runnable)
                {
                    return CommandStackRetVal.CONTINUE;
                }
                if (runnable.Index >= Entries.Length)
                {
                    queue.RunningStack.Pop();
                }
                if (queue.RunningStack.Count == 0)
                {
                    return CommandStackRetVal.STOP;
                }
                return CommandStackRetVal.CONTINUE;
            }
            catch (Exception ex)
            {
                FreneticScriptUtilities.CheckException(ex);
                if (!(ex is ErrorInducedException eie && string.IsNullOrEmpty(eie.Message)))
                {
                    try
                    {
                        if (ex is ErrorInducedException)
                        {
                            queue.HandleError(Entries[runnable.Index], ex.Message);
                        }
                        else
                        {
                            queue.HandleError(Entries[runnable.Index], "Internal exception:\n------\n" + ex.ToString() + "\n------");
                        }
                    }
                    catch (Exception ex2)
                    {
                        if (ex2 is ThreadAbortException)
                        {
                            throw;
                        }
                        if (!(ex2 is ErrorInducedException))
                        {
                            string message = ex2.ToString();
                            if (runnable.Debug <= DebugMode.MINIMAL)
                            {
                                queue.Engine.Context.BadOutput(message);
                                if (queue.Outputsystem != null)
                                {
                                    queue.Outputsystem.Invoke(message, MessageType.BAD);
                                }
                                runnable.Index = Entries.Length + 1;
                                queue.RunningStack.Clear();
                            }
                        }
                    }
                }
                if (queue.RunningStack.Count > 0)
                {
                    if (queue.RunningStack.Peek() == runnable)
                    {
                        queue.RunningStack.Pop();
                    }
                    return CommandStackRetVal.CONTINUE;
                }
                return CommandStackRetVal.STOP;
            }
            finally
            {
                runnable.CurrentQueue = null;
            }
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
            stacktrace.Append("ERROR: " + message + "\n    in script '" + entry.ScriptName + "' at line " + (entry.ScriptLine + 1)
                + ": (" + entry.Name + ")\n");
            queue.WaitingOn = null;
            CompiledCommandRunnable runnable = queue.RunningStack.Count > 0 ? queue.RunningStack.Peek() : null;
            DebugMode dbmode = runnable == null ? DebugMode.FULL : runnable.Debug;
            while (runnable != null)
            {
                for (int i = runnable.Index; i < runnable.Entry.Entries.Length; i++)
                {
                    CommandEntry entr = runnable.Entry.Entries[i];
                    if (entr.Command is TryCommand &&
                        entr.IsCallback)
                    {
                        entry.GoodOutput(queue, "Force-exiting try block.");
                        // TODO: queue.SetVariable("stack_trace", new TextTag(stacktrace.ToString().Substring(0, stacktrace.Length - 1)));
                        runnable.Index = i + 2;
                        throw new ErrorInducedException();
                    }
                }
                runnable.Index = runnable.Entry.Entries.Length + 1;
                queue.RunningStack.Pop();
                if (queue.RunningStack.Count > 0)
                {
                    runnable = queue.RunningStack.Peek();
                    queue.CurrentRunnable = runnable;
                    if (runnable.Index <= runnable.Entry.Entries.Length)
                    {
                        stacktrace.Append("    in script '" + runnable.Entry.Entries[runnable.Index - 1].ScriptName + "' at line " + (runnable.Entry.Entries[runnable.Index - 1].ScriptLine + 1)
                            + ": (" + runnable.Entry.Entries[runnable.Index - 1].Name + ")\n");
                    }
                }
                else
                {
                    runnable = null;
                    break;
                }
            }
            message = stacktrace.ToString().Substring(0, stacktrace.Length - 1);
            if (dbmode <= DebugMode.MINIMAL)
            {
                queue.Engine.Context.BadOutput(message);
                if (queue.Outputsystem != null)
                {
                    queue.Outputsystem.Invoke(message, MessageType.BAD);
                }
            }
            throw new ErrorInducedException("");
        }
    }

    /// <summary>
    /// Holds a <see cref="TemplateObject"/>.
    /// </summary>
    public class ObjectHolder // TODO: Remove!
    {
        /// <summary>
        /// The held object.
        /// </summary>
        public TemplateObject Internal;
    }
}
