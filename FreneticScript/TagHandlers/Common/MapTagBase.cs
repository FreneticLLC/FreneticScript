using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    // <--[explanation]
    // @Name Maps
    // @Description
    // A map is a relationship between textual names and object values.
    // TODO: Explain better!
    // -->

    public class MapTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base map[<MapTag>]
        // @Group Mathematics
        // @ReturnType MapTag
        // @Returns the specified input as a map.
        // <@link explanation maps>What are maps?<@/link>
        // -->
        public MapTagBase()
        {
            Name = "map";
            ResultTypeString = "maptag";
        }

        public static TemplateObject HandleOne(TagData data)
        {
            return MapTag.For(data, data.GetModifierObject(0));
        }
    }
}
