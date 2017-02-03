using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a tag object of unknown type.
    /// </summary>
    public class DynamicTag : TemplateObject
    {
        // <--[object]
        // @Type DynamicTag
        // @SubType TextTag
        // @Group Tag System
        // @Description Represents any object, dynamically.
        // -->

        // TODO: Explanation of dynamics!

        /// <summary>
        /// The represented tag object.
        /// </summary>
        public TemplateObject Internal;

        /// <summary>
        /// Constructs a new DynamicTag.
        /// </summary>
        /// <param name="type">The TemplateObject to base this DynamicTag off of.</param>
        public DynamicTag(TemplateObject type)
        {
            Internal = type;
        }

        /// <summary>
        /// Returns the type of this tag.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="input">The input text.</param>
        /// <returns>A TagTypeTag, or null.</returns>
        public static TagTypeTag For(TagData data, string input)
        {
            TagType type;
            if (data.TagSystem.Types.TryGetValue(input.ToLowerFast(), out type))
            {
                return new TagTypeTag(type);
            }
            return null;
        }

        /// <summary>
        /// Returns the type of this tag.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="input">The input object.</param>
        /// <returns>A TagTypeTag, or null.</returns>
        public static TagTypeTag For(TagData data, TemplateObject input)
        {
            return (input is TagTypeTag) ? (TagTypeTag)input : For(data, input.ToString());
        }

        /// <summary>
        /// The DynamicTag type.
        /// </summary>
        public const string TYPE = "dynamictag";

        /// <summary>
        /// All tag handlers for this tag type.
        /// </summary>
        public static Dictionary<string, TagSubHandler> Handlers = new Dictionary<string, TagSubHandler>();

        static DynamicTag()
        {
            // <--[tag]
            // @Name DynamicTag.as[<TagTypeTag>]
            // @Group Tag System
            // @ReturnType <Dynamic:SpecifiedType>
            // @Returns this tag as the specified type.
            // @Example "32" .as[IntegerTag] returns "32".
            // -->
            Handlers.Add("as", new TagSubHandler() { Handle = (data, obj) =>
            {
                TagTypeTag ttt = TagTypeTag.For(data, data.GetModifierObject(0));
                return ttt.Internal.TypeGetter(data, ((DynamicTag)obj).Internal);
            }, ReturnTypeString = null });
            // Documented in TextTag.
            Handlers.Add("duplicate", new TagSubHandler() { Handle = (data, obj) => new DynamicTag(((DynamicTag)obj).Internal), ReturnTypeString = "dynamictag" });
            // Documented in TextTag.
            Handlers.Add("type", new TagSubHandler() { Handle = (data, obj) => new TagTypeTag(data.TagSystem.Type_Dynamic), ReturnTypeString = "tagtypetag" });
            // Documented in TextTag.
            Handlers.Add("or_else", new TagSubHandler() { Handle = (data, obj) => obj, ReturnTypeString = "dynamictag" });
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
            return new TextTag(ToString()).Handle(data);
        }

        /// <summary>
        /// Returns the name of the tag type.
        /// </summary>
        /// <returns>The name.</returns>
        public override string ToString()
        {
            return Internal.ToString();
        }
    }
}
