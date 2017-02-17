using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'binary' tag base.
    /// </summary>
    public class BinaryTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base binary[<BinaryTag>]
        // @Group Common Base Types
        // @ReturnType BinaryTag
        // @Returns the input data as a BinaryTag.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public BinaryTagBase()
        {
            Name = "binary";
            ResultTypeString = "binarytag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return BinaryTag.CreateFor(data, data.GetModifierObject(0));
        }
    }
}
