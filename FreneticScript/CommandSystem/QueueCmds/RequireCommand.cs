//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;

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
        /// Generates a basic Require command entry.
        /// </summary>
        /// <param name="expectedMap">The expected variables maps.</param>
        /// <param name="scriptName">The script name.</param>
        /// <param name="line">The script line.</param>
        public CommandEntry GenerateEntry(MapTag expectedMap, string scriptName, int line)
        {
            string mapText = expectedMap.ToString();
            return new CommandEntry("require \"" + mapText + "\" \0AUTOGENNED", 0, 0, this,
                new List<Argument>() { new Argument(new TextArgumentBit(expectedMap)) }, Name, CommandPrefix.NONE, scriptName, line, "", Engine);
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
                TagType tagType;
                if (pair.Value is TagTypeTag tag)
                {
                    tagType = tag.Internal;
                }
                else if (!cent.System.TagSystem.Types.RegisteredTypes.TryGetValue(pair.Value.ToString(), out tagType))
                {
                    throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Invalid local variable type: " + pair.Value.ToString() + "!");
                }
                values.AddVariable(pair.Key, tagType);
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
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Call, REQUIRECOMMAND_OUTPUTSUCCESS);
            }
        }

        /// <summary>
        /// Represents the method <see cref="OutputSuccess(CommandQueue, CommandEntry)"/> in the class RequireCommand.
        /// </summary>
        public static MethodInfo REQUIRECOMMAND_OUTPUTSUCCESS = typeof(RequireCommand).GetMethod(nameof(OutputSuccess));

        /// <summary>
        /// Outputs success at the end of a require command execution.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">The command entry.</param>
        public static void OutputSuccess(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Require command passed.");
            }
        }

        /// <summary>
        /// Represents the method <see cref="CheckForValidity(CommandQueue, CommandEntry, ObjectHolder, string)"/> in the class RequireCommand.
        /// </summary>
        public static MethodInfo REQUIRECOMMAND_CHECKFORVALIDITY = typeof(RequireCommand).GetMethod(nameof(CheckForValidity));

        /// <summary>
        /// Checks an object holder's validity (non-null and contains non-null data), for CIL usage.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="objh">Object holder in question.</param>
        /// <param name="varn">Variable the object holder was gotten from.</param>
        public static void CheckForValidity(CommandQueue queue, CommandEntry entry, ObjectHolder objh, string varn)
        {
            if (objh.Internal == null)
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
