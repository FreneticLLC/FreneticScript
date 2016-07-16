using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.Arguments;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// The base for a command.
    /// </summary>
    public abstract class AbstractCommand
    {
        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name = "NAME:UNSET";

        /// <summary>
        /// The system that owns this command.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// A short explanation of the arguments of the command.
        /// </summary>
        public string Arguments = "ARGUMENTS:UNSET";

        /// <summary>
        /// A short explanation of what the command does.
        /// </summary>
        public string Description = "DESCRIPTION:UNSET";

        /// <summary>
        /// Whether the command is for debugging purposes.
        /// </summary>
        public bool IsDebug = false;

        /// <summary>
        /// Whether the 'break' command can be used on this command.
        /// </summary>
        public bool IsBreakable = false;

        /// <summary>
        /// Whether the command is part of a script's flow rather than for normal client use.
        /// </summary>
        public bool IsFlow = false;

        /// <summary>
        /// Whether the command can be &amp;waited on.
        /// </summary>
        public bool Waitable = false;

        /// <summary>
        /// Whether the command can be run off the primary tick.
        /// NOTE: These mostly have yet to be confirmed! They are purely theoretical!
        /// </summary>
        public bool Asyncable = false;

        /// <summary>
        /// How many arguments the command can have minimum.
        /// </summary>
        public int MinimumArguments = 0;

        /// <summary>
        /// How many arguments the command can have maximum.
        /// </summary>
        public int MaximumArguments = 100;

        /// <summary>
        /// The expected object type getters for a command.
        /// </summary>
        public List<Func<TemplateObject, TemplateObject>> ObjectTypes = null;
        
        /// <summary>
        /// Tests if the CommandEntry is valid for this command at pre-process time.
        /// </summary>
        /// <param name="entry">The entry to test</param>
        /// <returns>An error message (with tags), or null for none.</returns>
        public virtual string TestForValidity(CommandEntry entry)
        {
            if (entry.Arguments.Count < MinimumArguments)
            {
                return "Not enough arguments. Expected at least: " + MinimumArguments + ". Usage: " + TagParser.Escape(Arguments) + ", found only: " + TagParser.Escape(entry.AllOriginalArguments());
            }
            if (MaximumArguments != -1 && entry.Arguments.Count > MaximumArguments)
            {
                return "Too many arguments. Expected no more than: " + MaximumArguments + ". Usage: " + TagParser.Escape(Arguments) + ", found: " + TagParser.Escape(entry.AllOriginalArguments());
            }
            if (ObjectTypes != null)
            {
                for (int i = 0; i < entry.Arguments.Count; i++)
                {
                    if (entry.Arguments[i].Bits.Count == 1
                        && entry.Arguments[i].Bits[0] is TextArgumentBit
                        && i < ObjectTypes.Count)
                    {
                        TemplateObject obj = ObjectTypes[i].Invoke(((TextArgumentBit)entry.Arguments[i].Bits[0]).InputValue);
                        if (obj == null)
                        {
                            return "Invalid argument '" + TagParser.Escape(entry.Arguments[i].ToString())
                                + "', translates to NULL for this command's input expectation (Command is " + TagParser.Escape(entry.Command.Name) + ").";
                        }
                        ((TextArgumentBit)entry.Arguments[i].Bits[0]).InputValue = obj;
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets the follower (callback) entry for an entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public CommandEntry GetFollower(CommandEntry entry)
        {
            return new CommandEntry(entry.Name + " \0CALLBACK", entry.BlockStart, entry.BlockEnd, entry.Command, new List<Argument>() { new Argument() { Bits = new List<ArgumentBit>() {
                new TextArgumentBit("\0CALLBACK", false) } } }, entry.Name, 0, entry.ScriptName, entry.ScriptLine, entry.FairTabulation + "    ");
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public virtual void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            values.CallExecute(entry);
        }

        /// <summary>
        /// Adjust list of commands that formed by an inner block.
        /// </summary>
        /// <param name="entry">The producing entry.</param>
        /// <param name="input">The block of commands.</param>
        /// <param name="fblock">The final block to add to the entry.</param>
        public virtual void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            input.Add(GetFollower(entry));
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public abstract void Execute(CommandQueue queue, CommandEntry entry);

        /// <summary>
        /// Displays the usage information on a command to the console.
        /// </summary>
        /// <param name="queue">The associated queue.</param>
        /// <param name="entry">The CommandEntry data to show usage help to.</param>
        /// <param name="doError">Whether to end with an error.</param>
        public void ShowUsage(CommandQueue queue, CommandEntry entry, bool doError = true)
        {
            entry.Bad(queue, "<{text_color.emphasis}>" + TagParser.Escape(Name) + "<{text_color.base}>: " + TagParser.Escape(Description));
            entry.Bad(queue, "<{text_color.cmdhelp}>Usage: /" + TagParser.Escape(Name) + " " + TagParser.Escape(Arguments));
            if (IsDebug)
            {
                entry.Bad(queue, "Note: This command is intended for debugging purposes.");
            }
            if (doError)
            {
                queue.HandleError(entry, "Invalid arguments or not enough arguments!");
            }
        }
    }

    /// <summary>
    /// Holds all data needed for CIL adaptation.
    /// </summary>
    public class CILAdaptationValues
    {
        /// <summary>
        /// The compiling script.
        /// </summary>
        public CommandScript Script;

        /// <summary>
        /// Represents the field "CSEntry" in the class CompiledCommandRunnable.
        /// </summary>
        public static FieldInfo EntryField = typeof(CompiledCommandRunnable).GetField("CSEntry");

        /// <summary>
        /// Represents the field "Entries" in the class CommandStackEntry.
        /// </summary>
        public static FieldInfo EntriesField = typeof(CommandStackEntry).GetField("Entries");

        /// <summary>
        /// Represents the field "Command" in the class CommandEntry.
        /// </summary>
        public static FieldInfo Entry_CommandField = typeof(CommandEntry).GetField("Command");

        /// <summary>
        /// Represents the field "Internal" in the class IntHolder.
        /// </summary>
        public static FieldInfo IntHolder_InternalField = typeof(IntHolder).GetField("Internal");

        /// <summary>
        /// Represents the "Execute(queue, entry)" method.
        /// </summary>
        public static MethodInfo ExecuteMethod = typeof(AbstractCommand).GetMethod("Execute", new Type[] { typeof(CommandQueue), typeof(CommandEntry) });

        /// <summary>
        /// The type of the class CommandEntry.
        /// </summary>
        public static Type CommandEntryType = typeof(CommandEntry);

        /// <summary>
        /// The IL code generator.
        /// </summary>
        public ILGenerator ILGen;

        /// <summary>
        /// Load the entry onto the stack.
        /// </summary>
        public void LoadEntry(int entry)
        {
            ILGen.Emit(OpCodes.Ldarg_2);
            ILGen.Emit(OpCodes.Ldc_I4, entry);
            ILGen.Emit(OpCodes.Stfld, IntHolder_InternalField);
            ILGen.Emit(OpCodes.Ldarg_0);
            ILGen.Emit(OpCodes.Ldfld, EntryField);
            ILGen.Emit(OpCodes.Ldfld, EntriesField);
            ILGen.Emit(OpCodes.Ldc_I4, entry);
            ILGen.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// Load the queue variable onto the stack.
        /// </summary>
        public void LoadQueue()
        {
            ILGen.Emit(OpCodes.Ldarg_1);
        }

        /// <summary>
        /// Call the "Execute(queue, entry)" method with appropriate parameters.
        /// </summary>
        public void CallExecute(int entry)
        {
            LoadEntry(entry);
            ILGen.Emit(OpCodes.Ldfld, Entry_CommandField);
            LoadQueue();
            LoadEntry(entry); // Awkward -> avoid duplicate call?
            ILGen.Emit(OpCodes.Callvirt, ExecuteMethod);
        }
    }
}
