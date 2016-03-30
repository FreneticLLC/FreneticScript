using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a null value as a usable tag.
    /// </summary>
    public class NullTag : TemplateObject
    {
        // <--[object]
        // @Type NullTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents a null value.
        // -->

        /// <summary>
        /// Get a null tag if the input is null, or an internal null value if the input is not a null tag.
        /// </summary>
        /// <param name="input">The input to create or get a null tag from.</param>
        /// <returns>The null tag, or internal null.</returns>
        public static NullTag For(string input)
        {
            string low = input.ToLowerFast();
            if (low == "null")
            {
                return new NullTag();
            }
            return null;
        }

        /// <summary>
        /// Get a null tag if the input is null, or an internal null value if the input is not a null tag.
        /// </summary>
        /// <param name="input">The input to create or get a null tag from.</param>
        /// <returns>The null tag, or internal null.</returns>
        public static NullTag For(TemplateObject input)
        {
            return input is NullTag ? (NullTag)input: For(input.ToString());
        }

        /// <summary>
        /// Constructs a null tag.
        /// </summary>
        public NullTag()
        {
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
            switch (data[0])
            {
                // Documented in TextTag.
                case "duplicate":
                    return new NullTag().Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        /// <summary>
        /// Returns "&amp;{NULL}".
        /// </summary>
        /// <returns>"&amp;{NULL}".</returns>
        public override string ToString()
        {
            return "&{NULL}";
        }
    }
}
