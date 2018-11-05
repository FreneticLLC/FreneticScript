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
            ResultTypeString = "booleantag";
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
