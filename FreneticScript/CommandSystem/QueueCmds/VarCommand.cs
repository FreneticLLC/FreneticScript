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
            Arguments = "<variable> '=' <value> ['as' <type>]";
            Description = "Modifies a variable in the current queue.";
            IsFlow = true;
            MinimumArguments = 3;
            MaximumArguments = 5;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For, // TODO: Lowercase
                verify1,
                TemplateObject.Basic_For,
                verify2,
                TemplateObject.Basic_For // TODO: Lowercase
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
            if (!values.LVariables.Contains(larg))
            {
                values.LVariables.Add(larg);
            }
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            // TODO: Type verification? Or remove the 'as TYPE' system?
            values.MarkCommand(entry);
            CommandEntry cent = values.Entry.Entries[entry];
            int lvarloc = values.LocalVariableLocation(cent.Arguments[0].ToString().ToLowerFast());
            values.LoadQueue();
            values.ILGen.Emit(OpCodes.Ldc_I4, lvarloc);
            values.LoadEntry(entry);
            values.LoadQueue();
            values.ILGen.Emit(OpCodes.Ldc_I4, 2);
            // TODO: Debug this!?
            values.ILGen.Emit(OpCodes.Call, CILAdaptationValues.Entry_GetArgumentObjectMethod);
            values.ILGen.Emit(OpCodes.Call, CILAdaptationValues.Queue_SetLocalVarMethod);
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
            string variable = entry.GetArgument(queue, 0);
            TemplateObject value = entry.GetArgumentObject(queue, 2);
            if (entry.Arguments.Count == 5)
            {
                // TODO: Fix this tagdata nonsense
                TagData dat = new TagData(entry.Command.CommandSystem.TagSystem, new TagBit[0], TextStyle.Color_Simple, queue.CurrentEntry.Variables, queue.CurrentEntry.Debug, (o) => queue.HandleError(entry, o), null);
                TagTypeTag type = TagTypeTag.For(dat, entry.GetArgumentObject(queue, 4));
                TemplateObject obj = type.Internal.TypeGetter(dat, value);
                if (obj == null)
                {
                    queue.HandleError(entry, "Invalid object for specified type!");
                    return;
                }
                queue.SetVariable(variable, obj);
            }
            else
            {
                queue.SetVariable(variable, value);
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Variable updated!");
            }
        }
    }
}
