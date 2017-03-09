using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    // <--[explanation]
    // @Name Lists
    // @Description
    // A list is a type of tag that contains multiple <@link explanation text tags>Text Tags<@/link>.
    // TODO: Explain better!
    // -->

    /// <summary>
    /// Handles the 'list' tag base.
    /// </summary>
    public class ListTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base list[<ListTag>]
        // @Group Mathematics
        // @ReturnType ListTag
        // @Returns the specified input as a list.
        // <@link explanation lists>What are lists?<@/link>
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public ListTagBase()
        {
            Name = "list";
            ResultTypeString = "listtag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return ListTag.CreateFor(data.GetModifierObject(0));
        }
    }
}
