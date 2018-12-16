//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

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
    [ObjectMeta(Name = NullTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Tag System", Description = "Represents a null value.")]
    public class NullTag : TemplateObject
    {

        /// <summary>
        /// A reference to a pregenerated NullTag. As all NullTag objects are the same, this value can be used anywhere a NullTag is needed.
        /// </summary>
        public static readonly NullTag NULL_VALUE = new NullTag();

        /// <summary>
        /// A reference to a pregenerated DynamicTag referencing <see cref="NULL_VALUE"/>. As all NullTag objects are the same, this value can be used anywhere a DynamicTag of a NullTag is needed.
        /// </summary>
        public static readonly DynamicTag DYNAMIC_NULL_VALUE = new DynamicTag(NULL_VALUE);

        /// <summary>
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public override string GetTagTypeName()
        {
            return TYPE;
        }

        /// <summary>
        /// Return the type of this tag.
        /// </summary>
        /// <returns>The tag type.</returns>
        public override TagType GetTagType(TagTypes tagTypeSet)
        {
            return tagTypeSet.Type_Null;
        }

        /// <summary>
        /// Constructs a null tag.
        /// Don't use this. Use <see cref="NULL_VALUE"/>.
        /// </summary>
        public NullTag()
        {
        }

        /// <summary>
        /// The NullTag type.
        /// </summary>
        public const string TYPE = "null";

        /// <summary>
        /// Creates a NullTag for the given input data.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid time tag.</returns>
        public static NullTag CreateFor(TemplateObject input, TagData data)
        {
            return NULL_VALUE;
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NullTag Tag_Duplicate(NullTag obj, TagData data)
        {
            return NULL_VALUE;
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (NullTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(NullTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Types.Type_Null);
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
