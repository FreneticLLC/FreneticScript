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
        public override TemplateObject HandleOne(TagData data)
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
            public static SystemTag For(TagData data, TemplateObject input)
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
            /// All tag handlers for this tag type.
            /// </summary>
            public static Dictionary<string, TagSubHandler> Handlers = new Dictionary<string, TagSubHandler>();

            static SystemTag()
            {
                // Documented in TextTag.
                Handlers.Add("duplicate", new TagSubHandler() { Handle = (data, obj) => new SystemTag(), ReturnTypeString = "systemtag" });
                // Documented in TextTag.
                Handlers.Add("type", new TagSubHandler() { Handle = (data, obj) => new TagTypeTag(data.TagSystem.Type_System), ReturnTypeString = "tagtypetag" });
                // Documented in TextTag.
                Handlers.Add("or_else", new TagSubHandler() { Handle = (data, obj) => obj, ReturnTypeString = "systemtag" });
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
                // <--[tag]
                // @Name SystemTag.current_time_utc
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the current system time (UTC).
                // @Other preferred for time-related calculations.
                // -->
                Handlers.Add("current_time_utc", new TagSubHandler() { Handle = (data, obj) => new TimeTag(DateTimeOffset.UtcNow), ReturnTypeString = "timetag" });
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
                if (!data.HasFallback)
                {
                    data.Error("Invalid tag bit: '" + TagParser.Escape(data[0]) + "'!");
                }
                return new NullTag();
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
