//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a date-time as a usable tag.
    /// </summary>
    [ObjectMeta(Name = TimeTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Mathematics", Description = "Represents a point in time.",
        Others = new string[] { "Representation format is YYYY/MM/DD hh:mm:ss:tttt UTC+OO:oo ... ':tttt' is optional, all else is required."})]
    public class TimeTag : TemplateObject
    {

        /// <summary>
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public override string GetTagTypeName()
        {
            return TYPE;
        }

        /// <summary>
        /// Return the type of this tag.
        /// </summary>
        /// <returns>The tag type.</returns>
        public override TagType GetTagType(TagTypes tagTypeSet)
        {
            return tagTypeSet.Type_Time;
        }

        /// <summary>
        /// The DateTime this TimeTag represents.
        /// </summary>
        public DateTimeOffset Internal;

        // TODO: DurationTag, to represent spans of time.

        /// <summary>
        /// Get a time tag relevant to the specified input, erroring on the command system if invalid input is given (Returns null in that case!)
        /// </summary>
        /// <param name="input">The input text to create a time from.</param>
        /// <returns>The time tag, or null.</returns>
        public static TimeTag For(string input)
        {
            DateTimeOffset? dt = StringConversionHelper.StringToDateTime(input);
            if (dt == null)
            {
                return null;
            }
            return new TimeTag(dt.Value);
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
        public const string TYPE = "time";

        /// <summary>
        /// Creates a SystemTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid time tag.</returns>
        public static TimeTag CreateFor(TemplateObject input, TagData dat)
        {
            switch (input)
            {
                case TimeTag ttag:
                    return ttag;
                case DynamicTag dtag:
                    return CreateFor(dtag.Internal, dat);
                default:
                    return For(input.ToString());
            }
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeTag Tag_Duplicate(TimeTag obj, TagData data)
        {
            return new TimeTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (TimeTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(TimeTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Types.Type_Time);
        }

        // TODO: More 'Time Parts' tags!
        [TagMeta(TagType = TYPE, Name = "total_milliseconds", Group = "Time Parts", ReturnType = IntegerTag.TYPE, Returns = "The total number of milliseconds since Jan 1st, 0001 (UTC).",
            Examples = new string[] { "'0001/01/01 00:00:00:0000 UTC+00:00' .total_milliseconds returns '0'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Total_Milliseconds(TimeTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.ToUniversalTime().Ticks / 10000L);
        }

        [TagMeta(TagType = TYPE, Name = "short_date", Group = "Time Parts", ReturnType = TextTag.TYPE,
            Returns = "The date part of the time in a short-date format.",
            Examples = new string[] { "'2017/03/08 12:00:00:0000 UTC+00:00' .short_date returns '8/03/2017'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Date(TimeTag obj, TagData data)
        {
            return new TextTag(obj.Internal.Date.ToShortDateString());
        }

        [TagMeta(TagType = TYPE, Name = "long_date", Group = "Time Parts", ReturnType = TextTag.TYPE,
            Returns = "The date part of the time in a long-date format.",
            Examples = new string[] { "'2017/03/08 12:00:00:0000 UTC+00:00' .long_date returns 'Wednesday, 8 March 2017'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Long_Date(TimeTag obj, TagData data)
        {
            return new TextTag(obj.Internal.Date.ToLongDateString());
        }

        [TagMeta(TagType = TYPE, Name = "short_time", Group = "Time Parts", ReturnType = TextTag.TYPE, Returns = "The time in a HH:MM AM/PM format.",
            Examples = new string[] { "'2017/03/08 12:30:00:0000 UTC+00:00' .short_time returns '12:30 PM'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Time(TimeTag obj, TagData data)
        {
            return new TextTag(obj.Internal.DateTime.ToShortTimeString());
        }

        [TagMeta(TagType = TYPE, Name = "long_time", Group = "Time Parts", ReturnType = TextTag.TYPE,
            Returns = "The time in a HH:MM:SS AM/PM format.",
            Examples = new string[] { "'2017/03/08 12:30:00:0000 UTC+00:00' .long_time returns '12:30:00 PM'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Long_Time(TimeTag obj, TagData data)
        {
            return new TextTag(obj.Internal.DateTime.ToLongTimeString());
        }

        [TagMeta(TagType = TYPE, Name = "year", Group = "Time Parts", ReturnType = IntegerTag.TYPE,
            Returns = "The year represented in the time.",
            Examples = new string[] { "'2017/03/08 12:30:00:0000 UTC+00:00' .year returns '2017'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Year(TimeTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Year);
        }

        [TagMeta(TagType = TYPE, Name = "month", Group = "Time Parts", ReturnType = IntegerTag.TYPE,
            Returns = "The month of the year represented in the time.",
            Examples = new string[] { "'2017/03/08 12:30:00:0000 UTC+00:00' .month returns '3'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Month(TimeTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Month);
        }

        [TagMeta(TagType = TYPE, Name = "day", Group = "Time Parts", ReturnType = IntegerTag.TYPE,
            Returns = "The day of the month represented in the time.",
            Examples = new string[] { "'2017/03/08 12:30:00:0000 UTC+00:00' .day returns '8'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Day(TimeTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Day);
        }

        [TagMeta(TagType = TYPE, Name = "hours", Group = "Time Parts", ReturnType = IntegerTag.TYPE,
            Returns = "The number of hours represented in the time.",
            Others = new string[] { "Note this is 24 hour." },
            Examples = new string[] { "'2017/03/08 12:30:00:0000 UTC+00:00' .hours returns '12'." ,
                "'2017/03/08 14:30:00:0000 UTC+00:00' .hours returns '14'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Hours(TimeTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Hour);
        }

        [TagMeta(TagType = TYPE, Name = "minutes", Group = "Time Parts", ReturnType = IntegerTag.TYPE,
            Returns = "The number of minutes represented in the time.",
            Examples = new string[] { "'2017/03/08 12:30:00:0000 UTC+00:00' .minutes returns '30'."})]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Minutes(TimeTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Minute);
        }

        [TagMeta(TagType = TYPE, Name = "seconds", Group = "Time Parts", ReturnType = IntegerTag.TYPE,
            Returns = "The number of seconds represented in the time.",
            Examples = new string[] { "'2017/03/08 12:30:45:0000 UTC+00:00' .seconds returns '45'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Seconds(TimeTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Second);
        }

#pragma warning restore 1591

        /// <summary>
        /// Returns the a string representation of the date-time internally stored by this time tag.
        /// </summary>
        /// <returns>A string representation of the date-time.</returns>
        public override string ToString()
        {
            return StringConversionHelper.DateTimeToString(Internal, true);
        }
    }
}
