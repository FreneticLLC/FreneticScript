using System.Collections.Generic;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.Arguments
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
        /// Parse the argument part, reading any tags or other special data.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="vars">The variables for var tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <returns>The parsed final text.</returns>
        public abstract string Parse(string base_color, Dictionary<string, TemplateObject> vars, DebugMode mode);
    }
}
