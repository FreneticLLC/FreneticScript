using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a TagType, as a tag.
    /// </summary>
    public class TagTypeTag : TemplateObject
    {
        // <--[object]
        // @Type TagTypeTag
        // @SubType TextTag
        // @Group Tag System
        // @Description Represents the type of a tag.
        // -->

        /// <summary>
        /// The represented tag type.
        /// </summary>
        public TagType Internal;

        /// <summary>
        /// Constructs a new TagTypeTag.
        /// </summary>
        /// <param name="type">The TagType to base this TagTypeTag off of.</param>
        public TagTypeTag(TagType type)
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
            if (data.TagSystem.Types.TryGetValue(input.ToLowerFast(), out TagType type))
            {
                return new TagTypeTag(type);
            }
            return null;
        }

        /// <summary>
        /// The TagTypeTag type.
        /// </summary>
        public const string TYPE = "tagtypetag";

        /// <summary>
        /// Returns the input object as a TagTypeTag.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="input">The input object.</param>
        /// <returns>A TagTypeTag, or null.</returns>
        public static TagTypeTag For(TemplateObject input, TagData data)
        {
            return input as TagTypeTag ?? For(data, input.ToString());
        }

        #pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "for", Group = "Tag System", ReturnType = DynamicTag.TYPE, Returns = "A constructed instance of this tag type.",
            Examples = new string[] { "'numbertag' .for[3] returns '3'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_For(TagTypeTag obj, TagData data)
        {
            return new DynamicTag((obj.Internal.TypeGetter(data.GetModifierObject(0), data)));
        }

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Duplicate(TagTypeTag obj, TagData data)
        {
            return new TagTypeTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (TagTypeTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(TagTypeTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Type_TagType);
        }

#pragma warning restore 1591
        
        /// <summary>
        /// Returns the name of the tag type.
        /// </summary>
        /// <returns>The name.</returns>
        public override string ToString()
        {
            return Internal.TypeName;
        }
    }
}
