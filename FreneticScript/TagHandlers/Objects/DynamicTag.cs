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

        /// <summary>
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public override string GetTagTypeName()
        {
            return TYPE;
        }

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

        /// <summary>
        /// Creates a DynamicTag from the saved object.
        /// </summary>
        /// <param name="input">The input data.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>The object.</returns>
        public static DynamicTag CreateFromSaved(string input, TagData data)
        {
            return new DynamicTag(data.TagSystem.ParseFromSaved(input, data));
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_Duplicate(DynamicTag obj, TagData data)
        {
            return new DynamicTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (DynamicTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(DynamicTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Type_Null);
        }

        [TagMeta(TagType = TYPE, Name = "as", Group = "Dynamics", ReturnType = null, Returns = "The object as the specified type.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TemplateObject Tag_As(TemplateObject obj, TagData data)
        {
            // This will be specially compiled.
            throw new NotImplementedException();
        }

#pragma warning restore 1591

        /// <summary>
        /// Returns savable dynamic tag data.
        /// </summary>
        /// <returns>The data.</returns>
        public override string GetSavableString()
        {
            return TYPE + SAVE_MARK + Internal.GetSavableString();
        }

        /// <summary>
        /// Returns the dynamic tag data.
        /// </summary>
        /// <returns>The data.</returns>
        public override string ToString()
        {
            return Internal.ToString();
        }
    }
}
