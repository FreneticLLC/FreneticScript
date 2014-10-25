using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers.Objects
{
    public class TimeTag : TemplateObject
    {
        /// <summary>
        /// The text this TextTag represents.
        /// </summary>
        DateTime Time;

        public TimeTag(DateTime _time)
        {
            Time = _time;
        }

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
                // @Returns the total number of milliseconds since Jan 1st, 0001.
                // -->
                case "total_milliseconds":
                    return new TextTag(Time.Ticks / 10000L).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            return FreneticUtilities.DateTimeToString(Time);
        }
    }
}
