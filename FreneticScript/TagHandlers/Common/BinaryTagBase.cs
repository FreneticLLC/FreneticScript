using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    class BinaryTagBase : TemplateTagBase
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

        public override TemplateObject HandleOne(TagData data)
        {
            return BinaryTag.For(data, data.GetModifierObject(0));
        }
    }
}
