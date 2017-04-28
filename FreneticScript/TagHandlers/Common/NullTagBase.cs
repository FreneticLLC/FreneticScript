using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'null' tag base.
    /// </summary>
    public class NullTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base null
        // @Group Common Base Types
        // @ReturnType NullTag
        // @Returns a NullTag.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public NullTagBase()
        {
            Name = "null";
            ResultTypeString = NullTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return new NullTag();
        }
    }
}
