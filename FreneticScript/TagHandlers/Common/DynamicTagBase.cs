using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    public class DynamicTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base dynamic[<DynamicTag>]
        // @Group Common Base Types
        // @ReturnType DynamicTag
        // @Returns the input data as a DynamicTag.
        // -->

        public DynamicTagBase()
        {
            Name = "dynamic";
            ResultTypeString = "dynamictag";
        }

        public static TemplateObject HandleOne(TagData data)
        {
            return DynamicTag.CreateFor(data, data.GetModifierObject(0));
        }
    }
}
