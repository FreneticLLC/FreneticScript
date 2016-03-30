using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
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
        }

        /// <summary>
        /// Handles the 'time' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject Handle(TagData data)
        {
            TimeTag time = TimeTag.For(data.GetModifierObject(0));
            if (time == null)
            {
                data.Error("Invalid time object!");
                return null;
            }
            return time.Handle(data.Shrink());
        }
    }
}
