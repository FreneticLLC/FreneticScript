using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem.Arguments;

namespace Frenetic.TagHandlers
{
    /// <summary>
    /// Part of a tag.
    /// </summary>
    public class TagBit
    {
        /// <summary>
        /// The main portion of the tag.
        /// </summary>
        public string Key = null;

        /// <summary>
        /// The [Modifier] portion of a tag.
        /// </summary>
        public Argument Variable = null;

        /// <summary>
        /// Returns the tag bit as tag input text.
        /// </summary>
        /// <returns>The tag input text.</returns>
        public override string ToString()
        {
            if (Variable == null || Variable.Bits.Count == 0)
            {
                return Key;
            }
            else
            {
                return Key + "[" + Variable.ToString() + "]";
            }
        }
    }
}
