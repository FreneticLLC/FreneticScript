//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.CommonBases
{
    /// <summary>
    /// Handles the 'tagtype' tag base.
    /// </summary>
    public class TagTypeBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base tagtype[<BinaryTag>]
        // @Group Common Base Types
        // @ReturnType TagTypeTag
        // @Returns the input data as a TagTypeTag.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public TagTypeBase()
        {
            Name = "tagtype";
            ResultTypeString = TagTypeTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return TagTypeTag.For(data.GetModifierObjectCurrent(), data);
        }
    }
}
