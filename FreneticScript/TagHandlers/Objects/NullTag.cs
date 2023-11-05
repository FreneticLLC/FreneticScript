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

namespace FreneticScript.TagHandlers.Objects;

/// <summary>Represents a null value as a usable tag.</summary>
[ObjectMeta(Name = NullTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Tag System", Description = "Represents a null value.")]
public class NullTag : TemplateObject
{

    /// <summary>A reference to a pregenerated NullTag. As all NullTag objects are the same, this value can be used anywhere a NullTag is needed.</summary>
    public static readonly NullTag NULL_VALUE = new();

    /// <summary>
    /// A reference to a pregenerated DynamicTag referencing <see cref="NULL_VALUE"/>. As all NullTag objects are the same, this value can be used anywhere a DynamicTag of a NullTag is needed.
    /// </summary>
    public static readonly DynamicTag DYNAMIC_NULL_VALUE = new(NULL_VALUE);

    /// <summary>Return the type name of this tag.</summary>
    /// <returns>The tag type name.</returns>
    public override string GetTagTypeName()
    {
        return TYPE;
    }

    /// <summary>Return the type of this tag.</summary>
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

    /// <summary>The NullTag type.</summary>
    public const string TYPE = "null";

    /// <summary>Creates a NullTag for the given input data.</summary>
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

    /// <summary>Returns "&amp;{NULL}".</summary>
    /// <returns>"&amp;{NULL}".</returns>
    public override string ToString()
    {
        return "&{NULL}";
    }

    /// <summary>Gets a "clean" text form of an object for simpler output to debug logs, may have added colors or other details.</summary>
    /// <returns>The debug-friendly string.</returns>
    public override string GetDebugString()
    {
        return TextStyle.Warning + "&{NULL}";
    }
}
