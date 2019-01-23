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
    /// Gets a boolean tag.
    /// </summary>
    public class BooleanTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base boolean[<BooleanTag>]
        // @Group Common Base Types
        // @ReturnType BooleanTag
        // @Returns the input boolean as a BooleanTag.
        // -->

        /// <summary>
        /// Constructs the BooleanTagBase - for internal use only.
        /// </summary>
        public BooleanTagBase()
        {
            Name = "boolean";
            ResultTypeString = BooleanTag.TYPE;
        }

        /// <summary>
        /// Handles the 'boolean' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            return BooleanTag.For(data.GetModifierObjectCurrent(), data);
        }
    }
}
