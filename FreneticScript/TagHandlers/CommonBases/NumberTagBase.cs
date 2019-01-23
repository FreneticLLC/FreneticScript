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
    /// Handles the 'number' tag base.
    /// </summary>
    public class NumberTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base number[<NumberTag>]
        // @Group Common Base Types
        // @ReturnType NumberTag
        // @Returns the input number as a NumberTag.
        // -->

        /// <summary>
        /// Constructs the tag base.
        /// </summary>
        public NumberTagBase()
        {
            Name = "number";
            ResultTypeString = NumberTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return NumberTag.For(data.GetModifierObjectCurrent(), data);
        }
    }
}
