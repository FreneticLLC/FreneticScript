//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The require command.
    /// </summary>
    public class RequireCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the require command.
        /// </summary>
        public RequireCommand()
        {
            Name = "require";
            Arguments = "<map of variables, name:type|...>";
            Description = "Defines the input variables to a function call. Will give an error if a variable is not defined by the call.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                MapTag.For
            };
        }

        /// <summary>
        /// Prepares to adapt a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void PreAdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.Entry.Entries[entry];
            MapTag mt = MapTag.For(cent.Arguments[0].ToString());
            if (mt.Internal.Count == 0)
            {
                throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Empty map input to require!");
            }
            foreach (KeyValuePair<string, TemplateObject> pair in mt.Internal)
            {
                string tname = pair.Value.ToString();
                if (!cent.System.TagSystem.Types.TryGetValue(tname, out TagType t))
                {
                    throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Invalid local variable type: " + tname + "!");
                }
                values.AddVariable(pair.Key, t);
            }
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            bool debug = cent.DBMode <= DebugMode.FULL;
            MapTag mt = MapTag.For(cent.Arguments[0].ToString());
            if (mt.Internal.Count == 0)
            {
                throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Empty map input to require!");
            }
            int locArr = values.ILGen.DeclareLocal(typeof(ObjectHolder[]));
            values.LoadQueue();
            values.ILGen.Emit(OpCodes.Ldfld, CommandQueue.COMMANDQUEUE_CURRENTENTRY);
            values.ILGen.Emit(OpCodes.Ldfld, CompiledCommandStackEntry.CompiledCommandStackEntry_LocalVariables);
            values.ILGen.Emit(OpCodes.Stloc, locArr);
            foreach (string varn in mt.Internal.Keys)
            {
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Ldloc, locArr);
                values.ILGen.Emit(OpCodes.Ldc_I4, cent.VarLoc(varn));
                values.ILGen.Emit(OpCodes.Ldelem_Ref);
                values.ILGen.Emit(OpCodes.Ldstr, varn);
                values.ILGen.Emit(OpCodes.Call, REQUIRECOMMAND_CHECKFORVALIDITY);
            }
            if (debug)
            {
                values.LoadQueue();
                values.ILGen.Emit(OpCodes.Call, REQUIRECOMMAND_OUTPUTSUCCESS);
            }
        }

        /// <summary>
        /// Represents the method "OutputSuccess" in the class RequireCommand.
        /// </summary>
        public static MethodInfo REQUIRECOMMAND_OUTPUTSUCCESS = typeof(RequireCommand).GetMethod("OutputSuccess");

        /// <summary>
        /// Outputs success at the end of a require command execution.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        public static void OutputSuccess(CommandQueue queue)
        {
            if (queue.ShouldShowGood())
            {
                queue.GoodOutput("Require command passed.");
            }
        }

        /// <summary>
        /// Represents the method "CheckForValidity" in the class RequireCommand.
        /// </summary>
        public static MethodInfo REQUIRECOMMAND_CHECKFORVALIDITY = typeof(RequireCommand).GetMethod("CheckForValidity");

        /// <summary>
        /// Checks an object holder's validity (non-null and contains non-null data), for CIL usage.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="objh">Object holder in question.</param>
        /// <param name="varn">Variable the object holder was gotten from.</param>
        public static void CheckForValidity(CommandQueue queue, CommandEntry entry, ObjectHolder objh, string varn)
        {
            if (objh == null || objh.Internal == null)
            {
                queue.HandleError(entry, "A variable was required but not found: " + varn + "!");
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "The require command MUST be compiled!");
        }
    }
}
