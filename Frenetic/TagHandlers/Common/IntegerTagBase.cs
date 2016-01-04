using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    class IntegerTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base integer[<IntegerTag>]
        // @Group Common Base Types
        // @ReturnType IntegerTag
        // @Returns the input number as a IntegerTag.
        // -->

        public IntegerTagBase()
        {
            Name = "integer";
        }

        public override TemplateObject Handle(TagData data)
        {
            return IntegerTag.For(data, data.GetModifierObject(0)).Handle(data.Shrink());
        }
    }
}
