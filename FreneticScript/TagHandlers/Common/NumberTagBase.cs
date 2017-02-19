using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'number' tag base.
    /// </summary>
    public class NumberTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base number[<NumberTag>]
        // @Group Common Base Types
        // @ReturnType NumberTag
        // @Returns the input number as a NumberTag.
        // -->

        /// <summary>
        /// Constructs the tag base.
        /// </summary>
        public NumberTagBase()
        {
            Name = "number";
            ResultTypeString = "numbertag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return NumberTag.For(data.GetModifierObject(0), data);
        }
    }
}
