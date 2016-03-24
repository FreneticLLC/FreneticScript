using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

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
        /// Tests if the CommandEntry is valid for this command at pre-process time.
        /// </summary>
        /// <param name="entry">The entry to test</param>
        /// <returns>An error message (with tags), or null for none.</returns>
        public virtual string TestForValidity(CommandEntry entry)
        {
            if (entry.Arguments.Count < MinimumArguments)
            {
                return "Not enough arguments. Expected at least: " + MinimumArguments + ". Usage: " + TagParser.Escape(Arguments);
            }
            if (MaximumArguments != -1 && entry.Arguments.Count > MaximumArguments)
            {
                return "Too many arguments. Expected no more than: " + MaximumArguments + ". Usage: " + TagParser.Escape(Arguments);
            }
            return null;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public abstract void Execute(CommandEntry entry);

        /// <summary>
        /// Displays the usage information on a command to the console.
        /// </summary>
        /// <param name="entry">The CommandEntry data to get usage help from..</param>
        public static void ShowUsage(CommandEntry entry)
        {
            entry.Bad("<{text_color.emphasis}>" + TagParser.Escape(entry.Command.Name) + "<{text_color.base}>: " + TagParser.Escape(entry.Command.Description));
            entry.Bad("<{text_color.cmdhelp}>Usage: /" + TagParser.Escape(entry.Name) + " " + TagParser.Escape(entry.Command.Arguments));
            if (entry.Command.IsDebug)
            {
                entry.Bad("Note: This command is intended for debugging purposes.");
            }
            entry.Error("Invalid arguments or not enough arguments!");
        }
    }
}
