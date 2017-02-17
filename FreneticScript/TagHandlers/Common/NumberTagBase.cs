using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    public class NumberTagBase : TemplateTagBase
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
            ResultTypeString = "numbertag";
        }

        public static TemplateObject HandleOne(TagData data)
        {
            return NumberTag.For(data, data.GetModifierObject(0));
        }
    }
}
