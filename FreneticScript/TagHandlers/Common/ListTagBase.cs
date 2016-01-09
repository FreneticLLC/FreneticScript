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
        // @Base list[<TextTag>]
        // @Group Mathematics
        // @ReturnType ListTag<TextTag>
        // @Returns the specified text as a list.
        // <@link explanation lists>What are lists?<@/link>
        // -->
        public ListTagBase()
        {
            Name = "list";
        }

        public override TemplateObject Handle(TagData data)
        {
            TemplateObject modif = data.GetModifierObject(0);
            return (modif is ListTag ? modif: ListTag.For(modif.ToString())).Handle(data.Shrink());
        }
    }
}
