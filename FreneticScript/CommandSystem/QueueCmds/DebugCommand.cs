//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2018 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Debug command.
    /// </summary>
    public class DebugCommand : AbstractCommand
    {
        // <--[command]
        // @Name debug
        // @Arguments 'full'/'minimal'/'none'
        // @Short Changes the debug mode of the current queue.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Group Queue
        // @Minimum 1
        // @Maximum 1
        // @Description
        // Changes the debug mode of the current queue.
        // Can be: full, minimal, or none.
        // Full shows all debug, minimal shows only errors, none hides everything.
        // Note that this technically only applies to the current stack entry, not the entire queue.
        // Meaning, even if you <@link command call>call<@/link> a function in the current queue, debug mode for that function will not affect the debug of the calling function or script.
        // TODO: Explain more!
        // @Example
        // // This example sets the debug mode to full.
        // debug full;
        // @Example
        // // This example sets the debug mode to minimal.
        // debug minimal;
        // -->

        /// <summary>
        /// Constructs the debug command.
        /// </summary>
        public DebugCommand()
        {
            Name = "debug";
            Arguments = "'full'/'minimal'/'none'/'default'";
            Description = "Modifies the debug mode of the current queue.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "full" || inp == "minimal" || inp == "none" || inp == "default")
            {
                return new TextTag(inp);
            }
            return null;
        }

        /// <summary>
        /// Prepares to adapt a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        public override void PreAdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.Entry.Entries[entry];
            string larg = cent.Arguments[0].ToString().ToLowerFast();
            switch (larg)
            {
                case "full":
                    values.DBMode = DebugMode.FULL;
                    break;
                case "minimal":
                    values.DBMode = DebugMode.MINIMAL;
                    break;
                case "none":
                    values.DBMode = DebugMode.NONE;
                    break;
                case "default":
                    values.DBMode = values.Entry.Debug;
                    break;
                default:
                    throw new ErrorInducedException("Unknown debug mode: " + TextStyle.Color_Separate + larg + TextStyle.Color_Error + "!");
            }
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            if (cent.DBMode == DebugMode.FULL)
            {
                values.MarkCommand(entry);
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Call, Method_DebugOutput);
            }
        }

        /// <summary>
        /// A reference to <see cref="DebugOutput(CommandQueue, CommandEntry)"/>.
        /// </summary>
        public static MethodInfo Method_DebugOutput = typeof(DebugCommand).GetMethod(nameof(DebugOutput));

        /// <summary>
        /// Helper to output debug message informing of debug mode change.
        /// </summary>
        /// <param name="queue">The relevant queue.</param>
        /// <param name="entry">The relevant entry.</param>
        public static void DebugOutput(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Debug mode set to " + TextStyle.Color_Separate + entry.DBMode + TextStyle.Color_Outgood + ".");
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "The debug command MUST be compiled!");
        }
    }
}
