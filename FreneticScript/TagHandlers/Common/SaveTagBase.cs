using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'save' tag base.
    /// </summary>
    public class SaveTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base save[<DynamicTag>]
        // @Group Common Base Types
        // @ReturnType TextTag
        // @Returns the saved copy of a tag input.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public SaveTagBase()
        {
            Name = "save";
            ResultTypeString = TextTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return new TextTag(data.GetModifierObject(0).GetSavableString());
        }
    }
}
