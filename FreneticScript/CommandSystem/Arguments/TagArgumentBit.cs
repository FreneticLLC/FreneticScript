using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using System.Reflection;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// Part of an argument that contains tags.
    /// </summary>
    public class TagArgumentBit: ArgumentBit
    {
        /// <summary>
        /// The pieces that make up the tag.
        /// </summary>
        public TagBit[] Bits;

        /// <summary>
        /// The method that gets the result of this TagArgumentBit.
        /// </summary>
        public MethodInfo GetResultMethod;

        /// <summary>
        /// Calls the method directly.
        /// </summary>
        public MethodHandler GetResultHelper;

        /// <summary>
        /// Parse the tag.
        /// </summary>
        /// <param name="data">The relevant tag data.</param>
        /// <returns>The parsed final object.</returns>
        public delegate TemplateObject MethodHandler(TagData data);

        /// <summary>
        /// Constructs a TagArgumentBit.
        /// </summary>
        /// <param name="system">The relevant command system.</param>
        /// <param name="bits">The tag bits.</param>
        public TagArgumentBit(Commands system, TagBit[] bits)
        {
            Bits = bits;
            CommandSystem = system;
        }

        /// <summary>
        /// The tag to fall back on if this tag fails.
        /// </summary>
        public Argument Fallback;

        /// <summary>
        /// The starting point for this tag.
        /// </summary>
        public TemplateTagBase Start = null;

        /// <summary>
        /// Parse the argument part, reading any tags.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The relevant command stack entry, if any.</param>
        /// <returns>The parsed final object.</returns>
        public override TemplateObject Parse(string base_color, DebugMode mode, Action<string> error, CommandStackEntry cse)
        {
            return CommandSystem.TagSystem.ParseTags(this, base_color, mode, error, cse);
        }

        /// <summary>
        /// Returns the tag as tag input text.
        /// </summary>
        /// <returns>Tag input text.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<{");
            for (int i = 0; i < Bits.Length; i++)
            {
                sb.Append(Bits[i].ToString());
                if (i + 1 < Bits.Length)
                {
                    sb.Append(".");
                }
            }
            sb.Append("}>");
            return sb.ToString();
        }
    }
}
