using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

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

            [TagMeta(TagType = TYPE, Name = "current_time_utc", Group = "Utilities", ReturnType = TimeTag.TYPE, Returns = "The current system time (UTC).",
                Examples = new string[] { }, Others = new string[] { "Preferred for time-related calculations." })]
            public static TemplateObject Tag_Current_Time_UTC(TemplateObject obj, TagData data)
            {
                return new TimeTag(DateTimeOffset.UtcNow);
            }

#pragma warning restore 1591
            
            /// <summary>
            /// All tag handlers for this tag type.
            /// </summary>
            public static Dictionary<string, TagSubHandler> Handlers = new Dictionary<string, TagSubHandler>();

            static SystemTag()
            {
                // Documented in TextTag.
                Handlers.Add("duplicate", new TagSubHandler() { Handle = (data, obj) => new SystemTag(), ReturnTypeString = "systemtag" });
                // Documented in TextTag.
                Handlers.Add("type", new TagSubHandler() { Handle = (data, obj) => new TagTypeTag(data.TagSystem.Type_System), ReturnTypeString = "tagtypetag" });
                // <--[tag]
                // @Name SystemTag.os_version
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the name and version text of the operating system running this engine.
                // -->
                Handlers.Add("os_version", new TagSubHandler() { Handle = (data, obj) => new TextTag(Environment.OSVersion.VersionString), ReturnTypeString = "texttag" });
                // <--[tag]
                // @Name SystemTag.user
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the name of the system user running this engine.
                // -->
                Handlers.Add("user", new TagSubHandler() { Handle = (data, obj) => new TextTag(Environment.UserName), ReturnTypeString = "texttag" });
                // <--[tag]
                // @Name SystemTag.processor_count
                // @Group Utilities
                // @ReturnType IntegerTag
                // @Returns the number of (virtual) processors on this computer.
                // -->
                Handlers.Add("processor_count", new TagSubHandler() { Handle = (data, obj) => new IntegerTag(Environment.ProcessorCount), ReturnTypeString = "integertag" });
                // <--[tag]
                // @Name SystemTag.machine_name
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the name given to this computer.
                // -->
                Handlers.Add("machine_name", new TagSubHandler() { Handle = (data, obj) => new TextTag(Environment.MachineName), ReturnTypeString = "texttag" });
                // <--[tag]
                // @Name SystemTag.dotnet_version
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the system's .NET (CLR) version string.
                // -->
                Handlers.Add("dotnet_version", new TagSubHandler() { Handle = (data, obj) => new TextTag(Environment.Version.ToString()), ReturnTypeString = "texttag" });
                // TODO: Meta: Link the two current_time's at each other!
                // <--[tag]
                // @Name SystemTag.current_time_local
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the current system time (local).
                // @Other Preferred for displaying the current time.
                // -->
                Handlers.Add("current_time_local", new TagSubHandler() { Handle = (data, obj) => new TimeTag(DateTimeOffset.Now), ReturnTypeString = "timetag" });
                // <--[tag]
                // @Name SystemTag.total_ram_usage
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the total RAM usage of the program running. This is measured in bytes.
                // -->
                Handlers.Add("total_ram_usage", new TagSubHandler() { Handle = (data, obj) => new IntegerTag(GC.GetTotalMemory(false)), ReturnTypeString = "integertag" });
                // <--[tag]
                // @Name SystemTag.random_decimal
                // @Group Utilities
                // @ReturnType NumberTag
                // @Returns a random decimal number between zero and one.
                // -->
                Handlers.Add("random_decimal", new TagSubHandler() { Handle = (data, obj) => new NumberTag(data.TagSystem.CommandSystem.random.NextDouble()), ReturnTypeString = "numbertag" });
            }
            
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
