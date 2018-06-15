//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using System.Runtime.CompilerServices;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles the 'integer' tag base.
    /// </summary>
    public class IntegerTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base integer[<IntegerTag>]
        // @Group Common Base Types
        // @ReturnType IntegerTag
        // @Returns the input number as a IntegerTag.
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public IntegerTagBase()
        {
            Name = "integer";
            ResultTypeString = "integertag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // TODO: Auto-apply!
        public static TemplateObject HandleOne(TagData data)
        {
            return IntegerTag.For(data.GetModifierObjectCurrent(), data);
        }
    }
}
