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
        /// The DynamicTag type.
        /// </summary>
        public const string TYPE = "dynamictag";

        /// <summary>
        /// Creates a SystemTag for the given input data.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid time tag.</returns>
        public static DynamicTag CreateFor(TemplateObject input, TagData data)
        {
            return input as DynamicTag ?? new DynamicTag(input);
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        public static DynamicTag Tag_Duplicate(DynamicTag obj, TagData data)
        {
            return new DynamicTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (DynamicTag).")]
        public static TagTypeTag Tag_Type(DynamicTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Type_Null);
        }

        [TagMeta(TagType = TYPE, Name = "as", Group = "Dynamics", ReturnType = null, Returns = "The object as the specified type.")]
        public static TemplateObject Tag_As(TemplateObject obj, TagData data)
        {
            // This will be specially compiled.
            throw new NotImplementedException();
        }

#pragma warning restore 1591
        
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
