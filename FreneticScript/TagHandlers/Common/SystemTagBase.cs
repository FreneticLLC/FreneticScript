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
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override TemplateObject Handle(TagData data)
        {
            data.Shrink();
            if (data.Remaining == 0)
            {
                return new TextTag(ToString());
            }
            switch (data[0])
            {
                // <--[tag]
                // @Name SystemTag.os_version
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the name and version text of the operating system running this engine.s
                // -->
                case "os_version":
                    return new TextTag(Environment.OSVersion.VersionString).Handle(data.Shrink());
                // <--[tag]
                // @Name SystemTag.user
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the name of the system user running this engine.
                // -->
                case "user":
                    return new TextTag(Environment.UserName).Handle(data.Shrink());
                // <--[tag]
                // @Name SystemTag.processor_count
                // @Group Utilities
                // @ReturnType IntegerTag
                // @Returns the number of (virtual) processors on this computer.
                // -->
                case "processor_count":
                    return new IntegerTag(Environment.ProcessorCount).Handle(data.Shrink());
                // <--[tag]
                // @Name SystemTag.machine_name
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the name given to this computer.
                // -->
                case "machine_name":
                    return new TextTag(Environment.MachineName).Handle(data.Shrink());
                // <--[tag]
                // @Name SystemTag.dotnet_version
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns the system's .NET (CLR) version string.
                // -->
                case "dotnet_version":
                    return new TextTag(Environment.Version.ToString()).Handle(data.Shrink());
                // <--[tag]
                // @Name SystemTag.current_time_utc
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the current system time (UTC).
                // -->
                case "current_time_utc":
                    return new TimeTag(DateTimeOffset.UtcNow).Handle(data.Shrink());
                // TODO: Meta: Link the two current_time's at each other!
                // <--[tag]
                // @Name SystemTag.current_time
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the current system time (local).
                // -->
                case "current_time":
                    return new TimeTag(DateTimeOffset.Now).Handle(data.Shrink());
                // <--[tag]
                // @Name SystemTag.total_ram_usage
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the total RAM usage of the program running. This is measured in bytes.
                // -->
                case "total_ram_usage":
                    return new IntegerTag(GC.GetTotalMemory(false)).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }
    }
}
