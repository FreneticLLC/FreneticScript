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
        // @SubType NONE
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
            // TODO: Error?
            return null;
        }

        /// <summary>
        /// Get a null tag if the input is null, or an internal null value if the input is not a null tag.
        /// </summary>
        /// <param name="input">The input to create or get a null tag from.</param>
        /// <returns>The null tag, or internal null.</returns>
        public static NullTag For(TemplateObject input)
        {
            return input as NullTag ?? For(input.ToString());
        }

        /// <summary>
        /// Constructs a null tag.
        /// </summary>
        public NullTag()
        {
        }

        /// <summary>
        /// All tag handlers for this tag type.
        /// </summary>
        public static Dictionary<string, TagSubHandler> Handlers = new Dictionary<string, TagSubHandler>();

        static NullTag()
        {
            // Documented in TextTag.
            Handlers.Add("duplicate", new TagSubHandler() { Handle = (data, obj) => new NullTag(), ReturnTypeString = "nulltag" });
            // Documented in TextTag.
            Handlers.Add("type", new TagSubHandler() { Handle = (data, obj) => new TagTypeTag(data.TagSystem.Type_Null), ReturnTypeString = "tagtypetag" });
            // Documented in TextTag.
            Handlers.Add("or_else", new TagSubHandler() { Handle = (data, obj) => data.GetModifierObject(0) });
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
        /// Returns "&amp;{NULL}".
        /// </summary>
        /// <returns>"&amp;{NULL}".</returns>
        public override string ToString()
        {
            return "&{NULL}";
        }
    }
}
