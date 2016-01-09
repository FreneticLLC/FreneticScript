using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    class NumberTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base number[<NumberTag>]
        // @Group Common Base Types
        // @ReturnType NumberTag
        // @Returns the input number as a NumberTag.
        // -->

        public NumberTagBase()
        {
            Name = "number";
        }

        public override TemplateObject Handle(TagData data)
        {
            return NumberTag.For(data, data.GetModifierObject(0)).Handle(data.Shrink());
        }
    }
}
