using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using System.Reflection;
using System.Reflection.Emit;

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
            Arguments = "<variable> '=' <value> 'as' <type>";
            Description = "Modifies a variable in the current queue.";
            IsFlow = true;
            MinimumArguments = 3;
            MaximumArguments = 5;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For,
                verify1,
                TemplateObject.Basic_For,
                verify2,
                TemplateObject.Basic_For // TODO: TagTypeTag?
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
            string larg = cent.Arguments[0].ToString().ToLowerFast();
            if (values.LocalVariableLocation(larg) >= 0)
            {
                throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Duplicate local variable: " + larg + "!");
            }
            TagType t = null;
            if (cent.Arguments.Count >= 5)
            {
                string tname = cent.Arguments[4].ToString();
                if (!cent.System.TagSystem.Types.TryGetValue(tname, out t))
                {
                    throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Invalid local variable type: " + larg + "!");
                }
            }
            values.AddVariable(larg, t);
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            // TODO: Type verification!
            values.MarkCommand(entry);
            CommandEntry cent = values.Entry.Entries[entry];
            string type_name = cent.Arguments[4].ToString().ToLowerFast();
            TagType type = cent.System.TagSystem.Types[type_name];
            int lvarloc = cent.VarLoc(cent.Arguments[0].ToString().ToLowerFast());
            bool isCorrect = cent.Arguments[2].ReturnType()?.TypeName == type.TypeName;
            // This method:
            // queue.SetLocalVar(lvarloc, TYPE.CREATE_FOR(null, entry.GetArgumentObject(queue, 2)));
            // or:
            // queue.SetLocalVar(lvarloc, entry.GetArgumentObject(queue, 2));
            values.LoadQueue(); // Load the queue
            values.ILGen.Emit(OpCodes.Ldc_I4, lvarloc); // Prep the local variable location
            if (!isCorrect)
            {
                values.ILGen.Emit(OpCodes.Ldnull); // Prep a null (TagData)
            }
            values.LoadEntry(entry); // Load the entry
            values.LoadQueue(); // Load the queue
            values.ILGen.Emit(OpCodes.Ldc_I4, 2); // Prep a '2'
            // TODO: Debug output -> Only if compiled with debug on!
            values.ILGen.Emit(OpCodes.Call, CILAdaptationValues.Entry_GetArgumentObjectMethod); // Get the specified argument
            if (!isCorrect)
            {
                values.ILGen.Emit(OpCodes.Call, type.CreatorMethod); // Verify the type: Will either give back the object correctly, or throw an internal parsing exception (Probably not the best method...)
            }
            values.ILGen.Emit(OpCodes.Call, CILAdaptationValues.Queue_SetLocalVarMethod); // Push the result into the local var
        }

        TemplateObject verify1(TemplateObject input)
        {
            if (input.ToString() == "=")
            {
                return new TextTag("=");
            }
            return null;
        }

        TemplateObject verify2(TemplateObject input)
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
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "The var command MUST be compiled!");
        }
    }
}
