using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.Arguments
{
    /// <summary>
    /// Part of an argument that contains tags.
    /// </summary>
    public class TagArgumentBit: ArgumentBit
    {
        /// <summary>
        /// The pieces that make up the tag.
        /// </summary>
        public List<TagBit> Bits = new List<TagBit>();

        /// <summary>
        /// Parse the argument part, reading any tags.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="vars">The variables for var tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <returns>The parsed final text.</returns>
        public override string Parse(string base_color, Dictionary<string, TemplateObject> vars, DebugMode mode)
        {
            return CommandSystem.TagSystem.ParseTags(this, base_color, vars, mode);
        }

        /// <summary>
        /// Returns the tag as tag input text.
        /// </summary>
        /// <returns>Tag input text.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<{");
            for (int i = 0; i < Bits.Count; i++)
            {
                sb.Append(Bits[i].ToString());
            }
            sb.Append("}>");
            return sb.ToString();
        }
    }
}
