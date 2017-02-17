using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    class NullTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base null
        // @Group Common Base Types
        // @ReturnType NullTag
        // @Returns a NullTag.
        // -->

        public NullTagBase()
        {
            Name = "null";
            ResultTypeString = "nulltag";
        }

        public override TemplateObject HandleOne(TagData data)
        {
            return new NullTag();
        }
    }
}
