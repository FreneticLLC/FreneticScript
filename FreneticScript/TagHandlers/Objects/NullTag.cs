using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

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
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public override string GetTagTypeName()
        {
            return TYPE;
        }

        /// <summary>
        /// Constructs a null tag.
        /// </summary>
        public NullTag()
        {
        }

        /// <summary>
        /// The NullTag type.
        /// </summary>
        public const string TYPE = "nulltag";

        /// <summary>
        /// Creates a SystemTag for the given input data.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid time tag.</returns>
        public static NullTag CreateFor(TemplateObject input, TagData data)
        {
            return new NullTag();
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NullTag Tag_Duplicate(NullTag obj, TagData data)
        {
            return new NullTag();
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (NullTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(NullTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Type_Null);
        }
        
#pragma warning restore 1591
        
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
