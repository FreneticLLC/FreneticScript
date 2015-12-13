using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    /// <summary>
    /// Utility tags.
    /// </summary>
    public class UtilTags: TemplateTags
    {
        // <--[tagbase]
        // @Base util
        // @Group Utilities
        // @ReturnType UtilTag
        // @Returns a generic utility class full of specific helpful utility tags,
        // such as <@link tag UtilTag.random_decimal><{util.random_decimal}><@/link>.
        // -->

        /// <summary>
        /// Constructs the Utility tags.
        /// </summary>
        public UtilTags()
        {
            Name = "util";
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override string Handle(TagData data)
        {
            data.Shrink();
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name UtilTag.random_decimal
                // @Group Utilities
                // @ReturnType TextTag
                // @Returns a random decimal number between zero and one.
                // -->
                case "random_decimal":
                    return new TextTag(data.TagSystem.CommandSystem.random.NextDouble()).Handle(data.Shrink());
                // <--[tag]
                // @Name UtilTag.current_time_utc
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the current system time (UTC).
                // -->
                case "current_time_utc":
                    return new TimeTag(DateTime.UtcNow).Handle(data.Shrink());
                // TODO: Meta: Link the two current_time's at each other!
                // <--[tag]
                // @Name UtilTag.current_time
                // @Group Utilities
                // @ReturnType TimeTag
                // @Returns the current system time (local).
                // -->
                case "current_time":
                    return new TimeTag(DateTime.Now).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }
    }
}
