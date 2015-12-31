using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
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
            string modif = data.GetModifier(0);
            return BooleanTag.For(data, modif).Handle(data.Shrink());
        }
    }
}
