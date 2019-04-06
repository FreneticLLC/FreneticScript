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
    /// Handles the 'function' tag base.
    /// </summary>
    public class FunctionTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base function[<FunctionTag>]
        // @Group Common Base Types
        // @ReturnType FunctionTag
        // @Returns the input object as a FunctionTag.
        // -->

        /// <summary>
        /// Constructs the tag base.
        /// </summary>
        public FunctionTagBase()
        {
            Name = "function";
            ResultTypeString = FunctionTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static FunctionTag HandleOne(TagData data)
        {
            return FunctionTag.CreateFor(data.GetModifierObjectCurrent(), data);
        }
    }
}
