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
        public DateTimeOffset Internal;

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
        /// Get a time tag relevant to the specified input.
        /// </summary>
        /// <param name="data">The relevant tag data, if any.</param>
        /// <param name="input">The input to create a time from.</param>
        /// <returns>The time tag, or null.</returns>
        public static TimeTag For(TagData data, TemplateObject input)
        {
            return For(input);
        }
        
        /// <summary>
        /// Get a time tag relevant to the specified input.
        /// </summary>
        /// <param name="input">The input to create a time from.</param>
        /// <returns>The time tag, or null.</returns>
        public static TimeTag For(TemplateObject input)
        {
            return input as TimeTag ?? For(input.ToString());
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
        /// The TimeTag type.
        /// </summary>
        public const string TYPE = "timetag";

        /// <summary>
        /// Creates a SystemTag for the given input data.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid time tag.</returns>
        public static TemplateObject CreateFor(TagData data, TemplateObject input)
        {
            return (input as TimeTag) ?? For(input.ToString());
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        public static TemplateObject Tag_Duplicate(TagData data, TemplateObject obj)
        {
            return new TimeTag(((TimeTag)obj).Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (TimeTag).")]
        public static TemplateObject Tag_Type(TagData data, TemplateObject obj)
        {
            return new TagTypeTag(data.TagSystem.Type_Time);
        }

        // TODO: More 'Time Parts' tags!
        [TagMeta(TagType = TYPE, Name = "total_milliseconds", Group = "Time Parts", ReturnType = IntegerTag.TYPE, Returns = "The total number of milliseconds since Jan 1st, 0001 (UTC).",
            Examples = new string[] { "'0001/01/01 00:00:00:0000 UTC+00:00' .total_milliseconds returns 0." })]
        public static TemplateObject Tag_Current_Time_UTC(TagData data, TemplateObject obj)
        {
            return new IntegerTag(((TimeTag)obj).Internal.ToUniversalTime().Ticks / 10000L);
        }

#pragma warning restore 1591
        
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
            // TODO: Scrap!
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
