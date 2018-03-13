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
using System.Threading;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

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
        /// Represents the field "LocalVariables" in the class CompiledCommandStackEntry.
        /// </summary>
        public static FieldInfo COMPILEDCOMMANDSTACKENTRY_LOCALVARIABLES = typeof(CompiledCommandStackEntry).GetField("LocalVariables");

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
            CompiledCommandStackEntry ccse = (CompiledCommandStackEntry)MemberwiseClone();
            ccse.LocalVariables = new ObjectHolder[LocalVariables.Length];
            for (int i = 0; i < ccse.LocalVariables.Length; i++)
            {
                ccse.LocalVariables[i] = new ObjectHolder() { Internal = ccse.LocalVariables[i]?.Internal };
            }
            return ccse;
        }

        /// <summary>
        /// Run this command stack.
        /// </summary>
        /// <param name="queue">The queue to run under.</param>
        /// <returns>Whether to continue looping.</returns>
        public CommandStackRetVal Run(CommandQueue queue)
        {
            IntHolder c = new IntHolder() { Internal = 0 };
            try
            {
                MainCompiledRunnable.Run(queue, c, Entries, Index);
                Index = c.Internal + 1;
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
                if (ex is ThreadAbortException)
                {
                    throw;
                }
                if (!(ex is ErrorInducedException eie && string.IsNullOrEmpty(eie.Message)))
                {
                    try
                    {
                        if (ex is ErrorInducedException)
                        {
                            queue.HandleError(Entries[c.Internal], ex.Message);
                        }
                        else
                        {
                            queue.HandleError(Entries[c.Internal], "Internal exception: " + ex.ToString());
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
                                queue.CommandSystem.Output.BadOutput(message);
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
        }
    }
}
