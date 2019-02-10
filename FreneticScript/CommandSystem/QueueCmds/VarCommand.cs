//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using System.Reflection;
using System.Reflection.Emit;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The var command.
    /// </summary>
    public class VarCommand : AbstractCommand
    {
        // TODO: Meta

        /// <summary>
        /// Construct the var command.
        /// </summary>
        public VarCommand()
        {
            Name = "var";
            Arguments = "<variable> '=' <value> ['as' <type>]";
            Description = "Modifies a variable in the current queue.";
            IsFlow = true;
            MinimumArguments = 3;
            MaximumArguments = 5;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For,
                Verify1,
                TemplateObject.Basic_For,
                Verify2,
                TemplateObject.Basic_For // TODO: TagTypeTag?
            };
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
            if (values.LocalVariableLocation(larg) >= 0)
            {
                throw new ErrorInducedException("Duplicate local variable: " + larg + "!");
            }
            TagType t = null;
            if (cent.Arguments.Count >= 5)
            {
                string tname = cent.Arguments[4].ToString();
                if (!cent.System.TagSystem.Types.RegisteredTypes.TryGetValue(tname, out t))
                {
                    throw new ErrorInducedException("Invalid local variable type: " + larg + "!");
                }
            }
            else
            {
                t = cent.Arguments[2].ReturnType(values);
            }
            values.AddVariable(larg, t);
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            bool debug = cent.DBMode.ShouldShow(DebugMode.FULL);
            values.MarkCommand(entry);
            bool isCorrect = true;
            TagType type = null;
            TagType returnType = cent.Arguments[2].ReturnType(values);
            if (cent.Arguments.Count > 4)
            {
                string type_name = cent.Arguments[4].ToString().ToLowerFast();
                type = cent.System.TagSystem.Types.RegisteredTypes[type_name];
                isCorrect = returnType.TypeName == type.TypeName;
                returnType = type;
            }
            string lvarname = cent.Arguments[0].ToString().ToLowerFast();
            int lvarloc = cent.VarLoc(lvarname);
            // This method:
            // queue.SetLocalVar(lvarloc, TYPE.CREATE_FOR(null, entry.GetArgumentObject(queue, 2)));
            // or:
            // queue.SetLocalVar(lvarloc, entry.GetArgumentObject(queue, 2));
            int localInd = -1;
            if (debug)
            {
                localInd = values.ILGen.DeclareLocal(typeof(TemplateObject)); // Create variable 'o' for later usage.
            }
            values.LoadQueue(); // Load the queue
            values.ILGen.Emit(OpCodes.Ldc_I4, lvarloc); // Prep the local variable location
            values.LoadEntry(entry); // Load the entry
            values.LoadQueue(); // Load the queue
            values.ILGen.Emit(OpCodes.Ldc_I4, 2); // Prep a '2'
            values.ILGen.Emit(OpCodes.Call, CILAdaptationValues.Entry_GetArgumentObjectMethod); // Get the specified argument
            if (!isCorrect)
            {
                values.LoadTagData(); // Load a basic TagData object appropriate to the queue.
                values.ILGen.Emit(OpCodes.Call, type.CreatorMethod); // Verify the type: Will either give back the object correctly, or throw an internal parsing exception (Probably not the best method...)
            }
            if (debug) // If in debug mode...
            {
                values.ILGen.Emit(OpCodes.Dup); // Duplicate the result on the stack
                values.ILGen.Emit(OpCodes.Stloc, localInd); // Store it to the variable 'o'.
            }
            values.ILGen.Emit(OpCodes.Call, CILAdaptationValues.Queue_SetLocalVarMethod); // Push the result into the local var
            if (debug) // If in debug mode...
            {
                values.ILGen.Emit(OpCodes.Ldloc, localInd); // Load variable 'o'.
                values.ILGen.Emit(OpCodes.Ldstr, lvarname); // Load the variable name as a string.
                values.ILGen.Emit(OpCodes.Ldstr, returnType.TypeName); // Load the variable type name as a string.
                values.LoadQueue(); // Load the queue
                values.LoadEntry(entry); // Load the entry
                values.ILGen.Emit(OpCodes.Call, Method_DebugHelper); // Call the debug method
            }
        }

        /// <summary>
        /// References <see cref="DebugHelper(TemplateObject, string, string, CommandQueue, CommandEntry)"/>.
        /// </summary>
        public static MethodInfo Method_DebugHelper = typeof(VarCommand).GetMethod(nameof(DebugHelper));

        /// <summary>
        /// Helps debug output for the var command.
        /// </summary>
        /// <param name="res">The object saved as a var.</param>
        /// <param name="varName">The variable name stored into.</param>
        /// <param name="typeName">The variable type name.</param>
        /// <param name="queue">The queue.</param>
        /// <param name="entry">The entry.</param>
        public static void DebugHelper(TemplateObject res, string varName, string typeName, CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Stored variable '" + TextStyle.Separate + varName
                    + TextStyle.Base + "' with value: '" + TextStyle.Separate + res.GetDebugString()
                    + TextStyle.Base + "' as type: '" + TextStyle.Separate + typeName + TextStyle.Base + "'.");
            }
        }

        TemplateObject Verify1(TemplateObject input)
        {
            if (input.ToString() == "=")
            {
                return new TextTag("=");
            }
            return null;
        }

        TemplateObject Verify2(TemplateObject input)
        {
            if (input.ToString().ToLowerFast() == "as")
            {
                return new TextTag("as");
            }
            return null;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "The var command MUST be compiled!");
        }
    }
}
