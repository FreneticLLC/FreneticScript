using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using System.Runtime.CompilerServices;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'integer' tag base.
    /// </summary>
    public class IntegerTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base integer[<IntegerTag>]
        // @Group Common Base Types
        // @ReturnType IntegerTag
        // @Returns the input number as a IntegerTag.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public IntegerTagBase()
        {
            Name = "integer";
            ResultTypeString = "integertag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // TODO: Auto-apply!
        public static TemplateObject HandleOne(TagData data)
        {
            return IntegerTag.For(data.GetModifierObject(0), data);
        }
    }
}
