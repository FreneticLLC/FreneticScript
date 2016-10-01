using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Gets a boolean tag.
    /// </summary>
    public class BooleanTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base boolean[<BooleanTag>]
        // @Group Common Base Types
        // @ReturnType BooleanTag
        // @Returns the input boolean as a BooleanTag.
        // -->

        /// <summary>
        /// Constructs the BooleanTagBase - for internal use only.
        /// </summary>
        public BooleanTagBase()
        {
            Name = "boolean";
        }

        /// <summary>
        /// Handles the 'boolean' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject HandleOne(TagData data)
        {
            return BooleanTag.For(data, data.GetModifierObject(0));
        }
    }
}
