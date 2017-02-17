using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    public class BinaryTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base binary[<BinaryTag>]
        // @Group Common Base Types
        // @ReturnType BinaryTag
        // @Returns the input data as a BinaryTag.
        // -->

        public BinaryTagBase()
        {
            Name = "binary";
            ResultTypeString = "binarytag";
        }

        public static TemplateObject HandleOne(TagData data)
        {
            return BinaryTag.CreateFor(data, data.GetModifierObject(0));
        }
    }
}
