using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    class BooleanTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base boolean[<BooleanTag>]
        // @Group Common Base Types
        // @ReturnType BooleanTag
        // @Returns the input boolean as a BooleanTag.
        // -->

        public BooleanTagBase()
        {
            Name = "boolean";
        }

        public override TemplateObject Handle(TagData data)
        {
            return BooleanTag.For(data, data.GetModifierObject(0)).Handle(data.Shrink());
        }
    }
}
