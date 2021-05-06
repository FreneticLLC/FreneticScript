//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a TagType, as a tag.
    /// </summary>
    [ObjectMeta(Name = TagTypeTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Tag System", Description = "Represents the type of a tag.")]
    public class TagTypeTag : TemplateObject
    {

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
            return tagTypeSet.Type_TagType;
        }

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
        /// Helper validator to validate an argument as a tag-type tag.
        /// </summary>
        /// <param name="validator">The validation helper.</param>
        public static void Validator(ArgumentValidation validator)
        {
            string tagName = validator.ObjectValue.ToString().ToLowerFast();
            if (validator.Entry.TagSystem.Types.RegisteredTypes.TryGetValue(tagName, out TagType type))
            {
                validator.ObjectValue = type.TagForm;
            }
            else
            {
                validator.ErrorResult = "Unrecognized TagType '" + tagName + "'";
            }
        }

        /// <summary>
        /// Returns a TagTypeTag for the given text.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="input">The input text.</param>
        /// <returns>A TagTypeTag.</returns>
        public static TagTypeTag For(TagData data, string input)
        {
            if (data.TagSystem.Types.RegisteredTypes.TryGetValue(input.ToLowerFast(), out TagType type))
            {
                return type.TagForm;
            }
            data.Error("Unrecognized TagType '" + input + "'");
            return new TagTypeTag(data.TagSystem.Types.Type_Null);
        }

        /// <summary>
        /// Creates a TagTypeTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid TagTypeTag.</returns>
        public static TagTypeTag CreateFor(TemplateObject input, TagData dat)
        {
            return input switch
            {
                TagTypeTag tttag => tttag,
                DynamicTag dtag => CreateFor(dtag.Internal, dat),
                _ => For(dat, input.ToString()),
            };
        }

        /// <summary>
        /// The TagTypeTag type.
        /// </summary>
        public const string TYPE = "tagtype";

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
            return new DynamicTag((obj.Internal.TypeGetter(data.GetModifierObjectCurrent(), data)));
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
            return new TagTypeTag(data.TagSystem.Types.Type_TagType);
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
