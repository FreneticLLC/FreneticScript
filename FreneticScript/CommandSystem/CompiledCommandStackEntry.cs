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
    public class CompiledCommandStackEntry: CommandStackEntry
    {
        /// <summary>
        /// The compiled runner object.
        /// </summary>
        public CompiledCommandRunnable MainCompiledRunnable;

        /// <summary>
        /// Where in the CIL code each entry starts.
        /// </summary>
        public Label[] AdaptedILPoints;

        /// <summary>
        /// Represents the <see cref="CompiledCommandStackEntry.LocalVariables"/> field.
        /// </summary>
        public static readonly FieldInfo CompiledCommandStackEntry_LocalVariables = typeof(CompiledCommandStackEntry).GetField(nameof(CompiledCommandStackEntry.LocalVariables));

        /// <summary>
        /// Variables local to the compiled function.
        /// </summary>
        public ObjectHolder[] LocalVariables;
        
        /// <summary>
        /// Perfectly duplicates this stack entry.
        /// </summary>
        /// <returns>The newly duplicated stack entry.</returns>
        public CompiledCommandStackEntry Duplicate()
        {
            CompiledCommandStackEntry ccse = MemberwiseClone() as CompiledCommandStackEntry;
            ccse.LocalVariables = new ObjectHolder[LocalVariables.Length];
            for (int i = 0; i < ccse.LocalVariables.Length; i++)
            {
                ccse.LocalVariables[i] = new ObjectHolder() { Internal = LocalVariables[i].Internal };
            }
            ccse.IndexHelper = new IntHolder();
            return ccse;
        }

        /// <summary>
        /// Run this command stack.
        /// </summary>
        /// <param name="queue">The queue to run under.</param>
        /// <returns>Whether to continue looping.</returns>
        public CommandStackRetVal Run(CommandQueue queue)
        {
            CurrentQueue = queue;
            try
            {
                MainCompiledRunnable.Run(queue, IndexHelper, Entries, Index);
                Index = IndexHelper.Internal + 1;
                if (queue.Delayable && ((queue.Wait > 0f) || queue.WaitingOn != null))
                {
                    return CommandStackRetVal.BREAK;
                }
                Callback?.Invoke();
                if (queue.CommandStack.Count == 0)
                {
                    return CommandStackRetVal.BREAK;
                }
                if (queue.CommandStack.Peek() != this)
                {
                    return CommandStackRetVal.CONTINUE;
                }
                if (Index >= Entries.Length)
                {
                    queue.CommandStack.Pop();
                }
                if (queue.CommandStack.Count == 0)
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
                        Index = IndexHelper.Internal;
                        if (ex is ErrorInducedException)
                        {
                            queue.HandleError(Entries[Index], ex.Message);
                        }
                        else
                        {
                            queue.HandleError(Entries[Index], "Internal exception:\n------\n" + ex.ToString() + "\n------");
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
                            if (Debug <= DebugMode.MINIMAL)
                            {
                                queue.Engine.Context.BadOutput(message);
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
                if (queue.CommandStack.Count > 0)
                {
                    if (queue.CommandStack.Peek() == this)
                    {
                        queue.CommandStack.Pop();
                    }
                    return CommandStackRetVal.CONTINUE;
                }
                return CommandStackRetVal.STOP;
            }
            finally
            {
                CurrentQueue = null;
            }
        }
    }

    /// <summary>
    /// Holds a <see cref="TemplateObject"/>.
    /// </summary>
    public class ObjectHolder
    {
        /// <summary>
        /// The held object.
        /// </summary>
        public TemplateObject Internal;
    }
}
