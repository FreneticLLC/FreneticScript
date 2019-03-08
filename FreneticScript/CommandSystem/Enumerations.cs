//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Enumeration of modes describing the way a command saves.
    /// </summary>
    public enum CommandSaveMode
    {
        /// <summary>
        /// The command does not save.
        /// </summary>
        NO_SAVE = 0,
        /// <summary>
        /// The command can only save when a name is given.
        /// </summary>
        WHEN_NAME_SPECIFIED = 1,
        /// <summary>
        /// The command has a default save name value.
        /// </summary>
        DEFAULT_NAME = 2,
        /// <summary>
        /// The command must have a specified save name.
        /// </summary>
        MUST_SPECIFY = 3
    }

    /// <summary>
    /// An enumeration of the possible debug modes a queue can have.
    /// Lower values show more debug output. Higher values show less.
    /// </summary>
    public enum DebugMode : byte
    {
        /// <summary>
        /// Debug everything.
        /// </summary>
        FULL = 1,
        /// <summary>
        /// Only debug errors.
        /// </summary>
        MINIMAL = 2,
        /// <summary>
        /// Debug nothing.
        /// </summary>
        NONE = 3
    }

    /// <summary>
    /// Extension methods for DebugMode.
    /// </summary>
    public static class DebugModeExtensions
    {
        /// <summary>
        /// Whether this mode should show output of the specified mode.
        /// </summary>
        /// <param name="mode">This mode.</param>
        /// <param name="testShowMode">The specified mode to compare with.</param>
        /// <returns>Whether it should show.</returns>
        public static bool ShouldShow(this DebugMode mode, DebugMode testShowMode)
        {
            return mode <= testShowMode;
        }

        /// <summary>
        /// Whether this mode should show less output than the specified mode.
        /// </summary>
        /// <param name="mode">This mode.</param>
        /// <param name="testShowMode">The specified mode to compare with.</param>
        /// <returns>Whether it should show less output.</returns>
        public static bool ShowsLessThan(this DebugMode mode, DebugMode testShowMode)
        {
            return mode > testShowMode;
        }
    }

    /// <summary>
    /// Represents the return value from a command stack run call.
    /// </summary>
    public enum CommandStackRetVal : byte
    {
        /// <summary>
        /// Tells the queue to continue.
        /// </summary>
        CONTINUE = 1,
        /// <summary>
        /// Tells the queue to wait a while.
        /// </summary>
        BREAK = 2,
        /// <summary>
        /// Tells the queue to stop entirely.
        /// </summary>
        STOP = 3
    }

    /// <summary>
    /// Command output message types.
    /// </summary>
    public enum MessageType : int
    {
        /// <summary>
        /// No output type, null, 0.
        /// </summary>
        NUL = 0,
        /// <summary>
        /// Bad output type, 1.
        /// </summary>
        BAD = 1,
        /// <summary>
        /// Good output type, 2.
        /// </summary>
        GOOD = 2,
        /// <summary>
        /// Informational output type, 3.
        /// </summary>
        INFO = 3
    }

    /// <summary>
    /// Represents the types of command prefixing symbol.
    /// </summary>
    public enum CommandPrefix
    {
        /// <summary>
        /// No prefix.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Add to a command value, or temporarily enable.
        /// </summary>
        ADD = 1,
        /// <summary>
        /// Subtract from a command value, or end a temporary enable.
        /// </summary>
        SUBTRACT = 2,
        /// <summary>
        /// Flip a command value.
        /// </summary>
        FLIP = 3,
        /// <summary>
        /// Wait for the command to finish.
        /// </summary>
        WAIT = 4,
        /// <summary>
        /// Special internal marker: this command is a callback.
        /// </summary>
        CALLBACK = 5
    }

    /// <summary>
    /// Helpers for command prefixes.
    /// </summary>
    public static class CommandPrefixHelpers
    {
        /// <summary>
        /// Standard prefix characters, starting at ID 1.
        /// </summary>
        public const string PREFIXES = "+-!&";

        /// <summary>
        /// An array mapping ASCII characters to prefix values.
        /// </summary>
        public static readonly CommandPrefix[] BY_CHAR = new CommandPrefix[128];

        static CommandPrefixHelpers()
        {
            BY_CHAR['+'] = CommandPrefix.ADD;
            BY_CHAR['-'] = CommandPrefix.SUBTRACT;
            BY_CHAR['!'] = CommandPrefix.FLIP;
            BY_CHAR['&'] = CommandPrefix.WAIT;
        }

        /// <summary>
        /// Gets the <see cref="CommandPrefix"/> for a character. Returns <see cref="CommandPrefix.NONE"/> if the character is not a prefix character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>The prefix.</returns>
        public static CommandPrefix ForCharacter(char character)
        {
            return character < 128 ? BY_CHAR[character] : CommandPrefix.NONE;
        }

        /// <summary>
        /// Returns the character for the prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>The character.</returns>
        public static char Character(this CommandPrefix prefix)
        {
            return PREFIXES[(int)prefix - 1];
        }
    }

    /// <summary>
    /// Represents the types of object operation methods.
    /// </summary>
    public enum ObjectOperation : int
    {
        /// <summary>
        /// Adds a value to the object.
        /// Input parameters: TemplateObject, (ObjectEditSource)
        /// </summary>
        ADD = 0,
        /// <summary>
        /// Subtracts a value from the object.
        /// Input parameters: TemplateObject, (ObjectEditSource)
        /// </summary>
        SUBTRACT = 1,
        /// <summary>
        /// Multiplies the object by a value.
        /// Input parameters: TemplateObject, (ObjectEditSource)
        /// </summary>
        MULTIPLY = 2,
        /// <summary>
        /// Divides the object by a value.
        /// Input parameters: TemplateObject, (ObjectEditSource)
        /// </summary>
        DIVIDE = 3,
        /// <summary>
        /// Sets a subobject by name.
        /// Input parameters: TemplateObject, string, (ObjectEditSource)
        /// </summary>
        SET = 4,
        /// <summary>
        /// A get-sub-settable method returns a subobject by name.
        /// Input parameters: string, (ObjectEditSource)
        /// </summary>
        GETSUBSETTABLE = 5
    }
}
