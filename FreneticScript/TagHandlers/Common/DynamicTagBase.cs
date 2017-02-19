using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'dynamic' tag base.
    /// </summary>
    public class DynamicTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base dynamic[<DynamicTag>]
        // @Group Common Base Types
        // @ReturnType DynamicTag
        // @Returns the input data as a DynamicTag.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public DynamicTagBase()
        {
            Name = "dynamic";
            ResultTypeString = "dynamictag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return DynamicTag.CreateFor(data.GetModifierObject(0), data);
        }
    }
}
