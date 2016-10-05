using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a date-time as a usable tag.
    /// </summary>
    public class TimeTag : TemplateObject
    {
        // <--[object]
        // @Type TimeTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents a point in time.
        // -->

        /// <summary>
        /// The DateTime this TimeTag represents.
        /// </summary>
        DateTimeOffset Internal;

        /// <summary>
        /// Get a time tag relevant to the specified input, erroring on the command system if invalid input is given (Returns null in that case!)
        /// </summary>
        /// <param name="input">The input text to create a time from.</param>
        /// <returns>The time tag, or null.</returns>
        public static TimeTag For(string input)
        {
            DateTimeOffset? dt = FreneticScriptUtilities.StringToDateTime(input);
            if (dt == null)
            {
                return null;
            }
            return new TimeTag(dt.Value);
        }

        /// <summary>
        /// Get a time tag relevant to the specified input, erroring on the command system if invalid input is given (Returns null in that case!)
        /// </summary>
        /// <param name="data">The relevant tag data, if any.</param>
        /// <param name="input">The input to create a time from.</param>
        /// <returns>The time tag, or null.</returns>
        public static TimeTag For(TagData data, TemplateObject input)
        {
            return For(input);
        }
        
        /// <summary>
        /// Get a time tag relevant to the specified input, erroring on the command system if invalid input is given (Returns null in that case!)
        /// </summary>
        /// <param name="input">The input to create a time from.</param>
        /// <returns>The time tag, or null.</returns>
        public static TimeTag For(TemplateObject input)
        {
            return (input is TimeTag) ? (TimeTag)input : For(input.ToString());
        }

        /// <summary>
        /// Constructs a time tag.
        /// </summary>
        /// <param name="_time">The internal date-time to use.</param>
        public TimeTag(DateTimeOffset _time)
        {
            Internal = _time;
        }

        /// <summary>
        /// All tag handlers for this tag type.
        /// </summary>
        public static Dictionary<string, TagSubHandler> Handlers = new Dictionary<string, TagSubHandler>();

        static TimeTag()
        {
            // Documented in TextTag.
            Handlers.Add("duplicate", new TagSubHandler() { Handle = (data, obj) => new TimeTag(((TimeTag)obj).Internal), ReturnTypeString = "timetag" });
            // Documented in TextTag.
            Handlers.Add("type", new TagSubHandler() { Handle = (data, obj) => new TagTypeTag(data.TagSystem.Type_Time), ReturnTypeString = "tagtypetag" });
            // Documented in TextTag.
            Handlers.Add("or_else", new TagSubHandler() { Handle = (data, obj) => obj, ReturnTypeString = "timetag" });
            // <--[tag]
            // @Name TimeTag.total_milliseconds
            // @Group Time Parts
            // @ReturnType NumberTag
            // @Returns the total number of milliseconds since Jan 1st, 0001 (UTC).
            // @Example "0001/01/01 00:00:00:0000 UTC+00:00" .total_milliseconds returns 0.
            // -->
            Handlers.Add("total_milliseconds", new TagSubHandler() { Handle = (data, obj) => new IntegerTag(((TimeTag)obj).Internal.ToUniversalTime().Ticks / 10000L), ReturnTypeString = "integertag" });
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override TemplateObject Handle(TagData data)
        {
            if (data.Remaining == 0)
            {
                return this;
            }
            TagSubHandler handler;
            if (Handlers.TryGetValue(data[0], out handler))
            {
                return handler.Handle(data, this).Handle(data.Shrink());
            }
            return new TextTag(ToString()).Handle(data.Shrink());
        }
        
        /// <summary>
        /// Returns the a string representation of the date-time internally stored by this time tag.
        /// </summary>
        /// <returns>A string representation of the date-time.</returns>
        public override string ToString()
        {
            return FreneticScriptUtilities.DateTimeToString(Internal, true);
        }
    }
}
