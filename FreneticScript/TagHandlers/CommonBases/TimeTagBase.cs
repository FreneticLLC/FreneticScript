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
    /// Gets a time tag.
    /// </summary>
    public class TimeTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base time[<BooleanTag>]
        // @Group Common Base Types
        // @ReturnType TimeTag
        // @Returns the input time as a TimeTag.
        // -->

        /// <summary>
        /// Constructs the TimeTagBase - for internal use only.
        /// </summary>
        public TimeTagBase()
        {
            Name = "time";
            ResultTypeString = TimeTag.TYPE;
        }

        /// <summary>
        /// Handles the 'time' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TimeTag HandleOne(TagData data)
        {
            return TimeTag.CreateFor(data.GetModifierObjectCurrent(), data);
        }
    }
}
