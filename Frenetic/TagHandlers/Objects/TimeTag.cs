using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers.Objects
{
    /// <summary>
    /// Represents a date-time as usable tag.
    /// </summary>
    public class TimeTag : TemplateObject
    {
        /// <summary>
        /// The text this TextTag represents.
        /// </summary>
        DateTime Time;

        /// <summary>
        /// Constructs a time tag.
        /// </summary>
        /// <param name="_time">The internal date-time to use</param>
        public TimeTag(DateTime _time)
        {
            Time = _time;
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data</param>
        public override string Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name TimeTag.total_milliseconds
                // @Group Time Parts
                // @ReturnType TextTag
                // @Returns the total number of milliseconds since Jan 1st, 0001 (UTC).
                // @Example "0001/01/01 00:00:00:0000 UTC+00:00" .total_milliseconds returns 0.
                // -->
                case "total_milliseconds":
                    return new TextTag(Time.Ticks / 10000L).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        /// <summary>
        /// Returns the a string representation of the date-time internally stored by this time tag.
        /// </summary>
        /// <returns>A string representation of the date-time</returns>
        public override string ToString()
        {
            return FreneticUtilities.DateTimeToString(Time, true);
        }
    }
}
