using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
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

        public override string Handle(TagData data)
        {
            string modif = data.GetModifier(0);
            return NumberTag.For(data, modif).Handle(data.Shrink());
        }
    }
}
