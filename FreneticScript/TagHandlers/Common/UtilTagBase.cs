using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Utility tags.
    /// </summary>
    public class UtilTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base util
        // @Group Utilities
        // @ReturnType UtilTag
        // @Returns a generic utility class full of specific helpful utility tags,
        // such as <@link tag UtilTag.random_decimal><{util.random_decimal}><@/link>.
        // @TODO util->Utilities?
        // -->

        /// <summary>
        /// Constructs the Utility tags.
        /// </summary>
        public UtilTagBase()
        {
            Name = "util";
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
                // @Name UtilTag.random_decimal
                // @Group Utilities
                // @ReturnType NumberTag
                // @Returns a random decimal number between zero and one.
                // -->
                case "random_decimal":
                    return new NumberTag(data.TagSystem.CommandSystem.random.NextDouble()).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }
    }
}
