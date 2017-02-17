using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// An Argument in the command system.
    /// </summary>
    public class Argument
    {
        /// <summary>
        /// The parts that build up the argument.
        /// </summary>
        public List<ArgumentBit> Bits = new List<ArgumentBit>();

        /// <summary>
        /// Whether the argument was input with "quotes" around it.
        /// </summary>
        public bool WasQuoted = true;
        
        /// <summary>
        /// Gets the resultant type of this argument.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public TagType ReturnType(CILAdaptationValues values)
        {
            if (Bits.Count == 1)
            {
                return Bits[0].ReturnType(values);
            }
            else
            {
                return Bits[0].CommandSystem.TagSystem.Type_Text;
            }
        }

        /// <summary>
        /// Parse the argument, reading any tags or other special data.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The relevant command stack entry, if any.</param>
        /// <returns>The parsed final text.</returns>
        public TemplateObject Parse(string base_color, DebugMode mode, Action<string> error, CommandStackEntry cse)
        {
            // TODO: Compile this cleverly. This if and some other parts can probably be pre-computed!
            if (Bits.Count == 1)
            {
                return Bits[0].Parse(base_color, mode, error, cse);
            }
            StringBuilder built = new StringBuilder();
            for (int i = 0; i < Bits.Count; i++)
            {
                built.Append(Bits[i].Parse(base_color, mode, error, cse).ToString());
            }
            return new TextTag(built.ToString());
        }

        /// <summary>
        /// Returns the argument as plain input text.
        /// </summary>
        /// <returns>The plain input text.</returns>
        public override string ToString()
        {
            if (Bits.Count == 1)
            {
                return Bits[0].ToString();
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Bits.Count; i++)
            {
                sb.Append(Bits[i].ToString());
            }
            return sb.ToString();
        }
    }
}
