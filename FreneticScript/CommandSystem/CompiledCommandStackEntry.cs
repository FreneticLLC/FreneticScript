using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represetns a precompiled command stack entry.
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
        /// Run this command stack.
        /// </summary>
        /// <param name="queue">The queue to run under.</param>
        /// <returns>Whether to continue looping.</returns>
        public override CommandStackRetVal Run(CommandQueue queue)
        {
            IntHolder c = new IntHolder() { Internal = 0 };
            try
            {
                // TODO: Delayable stuff, etc.
                MainCompiledRunnable.Run(queue, c);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw ex;
                }
                if (!(ex is ErrorInducedException))
                {
                    try
                    {
                        queue.HandleError(Entries[c.Internal], "Internal exception: " + ex.ToString());
                    }
                    catch (Exception ex2)
                    {
                        if (ex is ThreadAbortException)
                        {
                            throw ex;
                        }
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
            return CommandStackRetVal.STOP;
        }
    }
}
