using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    // <--[explanation]
    // @Name Text Tags
    // @Description
    // TextTags are any random text, built into the tag system.
    // TODO: Explain better
    // TODO: Link tag system explanation
    // -->
    class TextTags : TemplateTags // TODO: Tags -> TagBase
    {
        // <--[tagbase]
        // @Base text[<TextTag>]
        // @Group Common Base Types
        // @ReturnType TextTag
        // @Returns the input text as a TextTag.
        // <@link explanation Text Tags>What are text tags?<@/link>
        // -->

        public TextTags()
        {
            Name = "text";
        }

        public override string Handle(TagData data)
        {
            string modif = data.GetModifier(0);
            return new TextTag(modif).Handle(data.Shrink());
        }
    }
}
