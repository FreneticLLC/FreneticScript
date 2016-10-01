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
    class ListTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base list[<ListTag>]
        // @Group Mathematics
        // @ReturnType ListTag
        // @Returns the specified input as a list.
        // <@link explanation lists>What are lists?<@/link>
        // -->
        public ListTagBase()
        {
            Name = "list";
            ResultTypeString = "listtag";
        }

        public override TemplateObject HandleOne(TagData data)
        {
            return ListTag.For(data.GetModifierObject(0));
        }
    }
}
