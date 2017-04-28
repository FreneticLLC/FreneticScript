using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using System.Runtime.CompilerServices;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// System tags.
    /// </summary>
    public class SystemTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base system
        // @Group Utilities
        // @ReturnType SystemTag
        // @Returns a generic utility class full of specific helpful system-related tags.
        // -->

        /// <summary>
        /// Constructs the system tags.
        /// </summary>
        public SystemTagBase()
        {
            Name = "system";
            ResultTypeString = "systemtag";
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            return new SystemTag();
        }

        /// <summary>
        /// Helper for system-related tags.
        /// </summary>
        public class SystemTag : TemplateObject
        {
            // <--[object]
            // @Type SystemTag
            // @SubType TextTag
            // @Group Utilities
            // @Description Holds system-related helped tags.
            // -->

            /// <summary>
            /// Return the type name of this tag.
            /// </summary>
            /// <returns>The tag type name.</returns>
            public override string GetTagTypeName()
            {
                return TYPE;
            }
            
            /// <summary>
            /// Gets a system tag. Shouldn't be used.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="input">The input.</param>
            public static SystemTag For(TagData data, string input)
            {
                CVar tcv = data.TagSystem.CommandSystem.Output.CVarSys.Get(input);
                if (tcv == null)
                {
                    data.Error("Invalid CVar specified!");
                    return null;
                }
                return new SystemTag();
            }

            /// <summary>
            /// Gets a system tag. Shouldn't be used.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="input">The input.</param>
            public static SystemTag For(TemplateObject input, TagData data)
            {
                return input is SystemTag ? (SystemTag)input : For(data, input.ToString());
            }

            /// <summary>
            /// Constructs a System tag.
            /// </summary>
            public SystemTag()
            {
            }

            /// <summary>
            /// The SystemTag type.
            /// </summary>
            public const string TYPE = "systemtag";

            /// <summary>
            /// Creates a SystemTag for the given input data.
            /// </summary>
            /// <param name="data">The tag data.</param>
            /// <param name="text">The text input.</param>
            /// <returns>A valid system tag.</returns>
            public static TemplateObject CreateFor(TagData data, TemplateObject text)
            {
                return new SystemTag();
            }

#pragma warning disable 1591

            [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TagTypeTag Tag_Duplicate(TagTypeTag obj, TagData data)
            {
                return new TagTypeTag(obj.Internal);
            }

            [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (SystemTag).")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TagTypeTag Tag_Type(IntegerTag obj, TagData data)
            {
                return new TagTypeTag(data.TagSystem.Type_System);
            }

            [TagMeta(TagType = TYPE, Name = "current_time_utc", Group = "Utilities", ReturnType = TimeTag.TYPE, Returns = "The current system time (UTC).",
                Others = new string[] { "Preferred for time-related calculations." })]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TimeTag Tag_Current_Time_UTC(SystemTag obj, TagData data)
            {
                return new TimeTag(DateTimeOffset.UtcNow);
            }

            [TagMeta(TagType = TYPE, Name = "current_time_local", Group = "Utilities", ReturnType = TimeTag.TYPE,
                Returns = "The current local system time.", Others = new string[] { "Preferred for displaying the current time." })]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TimeTag Tag_Current_Time_Local(SystemTag obj, TagData data)
            {
                return new TimeTag(DateTimeOffset.Now);
            }

            [TagMeta(TagType = TYPE, Name = "os_version", Group = "Utilities", ReturnType = TextTag.TYPE,
                Returns = "The name and version text of the operating system running this engine.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TextTag Tag_OS_Version(SystemTag obj, TagData data)
            {
                return new TextTag(Environment.OSVersion.VersionString);
            }

            [TagMeta(TagType = TYPE, Name = "user", Group = "Utilities", ReturnType = TextTag.TYPE,
                Returns = "The name of the system user running this engine.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TextTag Tag_User(SystemTag obj, TagData data)
            {
                return new TextTag(Environment.UserName);
            }

            [TagMeta(TagType = TYPE, Name = "processor_count", Group = "Utilities", ReturnType = IntegerTag.TYPE,
                Returns = "The number of (virtual) processors on this computer.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IntegerTag Tag_Processor_Count(SystemTag obj, TagData data)
            {
                return new IntegerTag(Environment.ProcessorCount);
            }

            [TagMeta(TagType = TYPE, Name = "machine_name", Group = "Utilities", ReturnType = TextTag.TYPE,
                Returns = "The name given to this computer.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TextTag Tag_Machine_Name(SystemTag obj, TagData data)
            {
                return new TextTag(Environment.MachineName);
            }

            [TagMeta(TagType = TYPE, Name = "dotnet_version", Group = "Utilities", ReturnType = TextTag.TYPE,
                Returns = "The system's .NET (CLR) version string.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static TextTag Tag_DotNET_Version(SystemTag obj, TagData data)
            {
                return new TextTag(Environment.Version.ToString());
            }

            [TagMeta(TagType = TYPE, Name = "total_ram_usage", Group = "Utilities", ReturnType = IntegerTag.TYPE,
                Returns = "The total RAM usage of the program running. This is measured in bytes.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static IntegerTag Tag_Total_RAM_Usage(SystemTag obj, TagData data)
            {
                return new IntegerTag(GC.GetTotalMemory(false));
            }

            [TagMeta(TagType = TYPE, Name = "random_decimal", Group = "Utilities", ReturnType = NumberTag.TYPE,
                Returns = "A random decimal number between zero and one.")]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static NumberTag Tag_Random_Decimal(SystemTag obj, TagData data)
            {
                return new NumberTag(data.TagSystem.CommandSystem.random.NextDouble());
            }

#pragma warning restore 1591
            
            /// <summary>
            /// Returns "System".
            /// </summary>
            public override string ToString()
            {
                return "System";
            }
        }
    }
}
