//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.CommonBases
{
    /// <summary>Handles the 'integer' tag base.</summary>
    public class IntegerTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base integer[<IntegerTag>]
        // @Group Common Base Types
        // @ReturnType IntegerTag
        // @Returns the input number as a IntegerTag.
        // -->

        /// <summary>Constructs the tag base data.</summary>
        public IntegerTagBase()
        {
            Name = "integer";
            ResultTypeString = IntegerTag.TYPE;
        }

        /// <summary>Handles the base input for a tag.</summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // TODO: Auto-apply!
        public static long HandleOne(TagData data)
        {
            return IntegerTag.CreateFor_Raw(data.GetModifierObjectCurrent(), data);
        }
    }
}
