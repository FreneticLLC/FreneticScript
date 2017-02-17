using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'tagtype' tag base.
    /// </summary>
    public class TagTypeBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base tagtype[<BinaryTag>]
        // @Group Common Base Types
        // @ReturnType TagTypeTag
        // @Returns the input data as a TagTypeTag.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public TagTypeBase()
        {
            Name = "tagtype";
            ResultTypeString = "tagtypetag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return TagTypeTag.For(data, data.GetModifierObject(0));
        }
    }
}
