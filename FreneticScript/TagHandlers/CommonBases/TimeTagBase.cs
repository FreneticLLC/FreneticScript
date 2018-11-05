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
            ResultTypeString = "timetag";
        }

        /// <summary>
        /// Handles the 'time' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            return TimeTag.CreateFor(data.GetModifierObjectCurrent(), data);
        }
    }
}
