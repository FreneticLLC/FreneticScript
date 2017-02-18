using System;
using System.Collections.Generic;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// Part of an Argument, abstract.
    /// </summary>
    public abstract class ArgumentBit
    {
        /// <summary>
        /// The relevant command system.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// Gets the resultant type of this argument bit.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public abstract TagType ReturnType(CILAdaptationValues values);

        /// <summary>
        /// Parse the argument part, reading any tags or other special data.
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        /// <returns>The parsed final text.</returns>
        public abstract TemplateObject Parse(Action<string> error, CompiledCommandStackEntry cse);
    }
}
