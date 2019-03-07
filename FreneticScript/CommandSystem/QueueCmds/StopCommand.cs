//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FreneticScript.ScriptSystems;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // TODO: Meta!

    /// <summary>
    /// The Stop command.
    /// </summary>
    public class StopCommand : AbstractCommand
    {
        /// <summary>
        /// Constructs the stop command.
        /// </summary>
        public StopCommand()
        {
            Name = "stop";
            Arguments = "";
            Description = "Immediately stops the current script.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 0;
        }

        /// <summary>
        /// Represents the <see cref="DebugStop(CommandQueue, CommandEntry)"/> method.
        /// </summary>
        public static MethodInfo DebugStopMethod = typeof(StopCommand).GetMethod(nameof(DebugStop));

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            bool db = cent.DBMode <= DebugMode.FULL;
            if (db)
            {
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Call, DebugStopMethod);
            }
            values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[values.Entry.AdaptedILPoints.Length - 1]);
        }

        /// <summary>
        /// Shows debug for a stop command.
        /// </summary>
        /// <param name="queue">The command queue.</param>
        /// <param name="entry">The command entry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugStop(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Stopping script.");
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "Cannot Execute() a stop command, must compile!");
        }
    }
}
