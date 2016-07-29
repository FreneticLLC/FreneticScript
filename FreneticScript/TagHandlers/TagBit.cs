using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.TagHandlers
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
        /// The original input variable.
        /// </summary>
        public Argument OVar = null;

        /// <summary>
        /// The handler that could handle this tag, if any is available.
        /// </summary>
        public TagSubHandler Handler = null;

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
            else if (Key.StartsWith("\0"))
            {
                return "[" + OVar.ToString() + "]";
            }
            else
            {
                return Key + "[" + Variable.ToString() + "]";
            }
        }
    }
}
