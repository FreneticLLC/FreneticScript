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

namespace FreneticScript.TagHandlers.Objects;

/// <summary>Represents a true or false as a usable tag.</summary>
/// <remarks>
/// Do not construct directly! Use <see cref="ForBool(bool)"/>.
/// </remarks>
/// <param name="_val">The internal boolean to use.</param>
[ObjectMeta(Name = BooleanTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Mathematics", Description = "Represents a 'true' or 'false'.")]
public class BooleanTag(bool _val) : TemplateObject
{

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
        return tagTypeSet.Type_Boolean;
    }

    /// <summary>The boolean this tag represents.</summary>
    public readonly bool Internal = _val;

    /// <summary>A true value.</summary>
    public static readonly BooleanTag TRUE = new(true);

    /// <summary>A false value.</summary>
    public static readonly BooleanTag FALSE = new(false);

    /// <summary>
    /// Gets a boolean tag for the input bool value.
    /// Never null!
    /// </summary>
    /// <param name="val">The boolean value.</param>
    /// <returns>A valid BooleanTag.</returns>
    public static BooleanTag ForBool(bool val)
    {
        return val ? TRUE : FALSE;
    }

    /// <summary>Helper validator to validate an argument as a boolean tag.</summary>
    /// <param name="validator">The validation helper.</param>
    public static void Validator(ArgumentValidation validator)
    {
        validator.ObjectValue = For(validator.ObjectValue, validator.ErrorAction);
    }

    /// <summary>
    /// Get a boolean tag relevant to the specified input, erroring on the command system if invalid input is given (Returns false in that case).
    /// Never null!
    /// </summary>
    /// <param name="err">Error call if something goes wrong.</param>
    /// <param name="input">The input text to create a boolean from.</param>
    /// <returns>The boolean tag.</returns>
    public static BooleanTag For(Action<string> err, string input)
    {
        string low = input.ToLowerFast();
        if (low == "true")
        {
            return TRUE;
        }
        if (low == "false")
        {
            return FALSE;
        }
        err("Invalid boolean: '" + input + "'!");
        return null;
    }

    /// <summary>
    /// Get a boolean tag relevant to the specified input, erroring on the command system if invalid input is given (Returns false in that case).
    /// Never null!
    /// </summary>
    /// <param name="err">Error call if something goes wrong.</param>
    /// <param name="input">The input to create or get a boolean from.</param>
    /// <returns>The boolean tag.</returns>
    public static BooleanTag For(TemplateObject input, Action<string> err)
    {
        return input switch
        {
            BooleanTag itag => itag,
            DynamicTag dtag => For(dtag.Internal, err),
            _ => For(err, input.ToString()),
        };
    }

    /// <summary>
    /// Get a boolean tag relevant to the specified input, erroring on the command system if invalid input is given (Returns false in that case).
    /// Never null!
    /// </summary>
    /// <param name="dat">The TagData used to construct this BooleanTag.</param>
    /// <param name="input">The input text to create a boolean from.</param>
    /// <returns>The boolean tag.</returns>
    public static BooleanTag For(TagData dat, string input)
    {
        string low = input.ToLowerFast();
        if (low == "true")
        {
            return TRUE;
        }
        if (low == "false")
        {
            return FALSE;
        }
        dat.Error("Invalid boolean: '" + input + "'!");
        return null;
    }

    /// <summary>
    /// Get a boolean tag relevant to the specified input, erroring on the command system if invalid input is given (Returns false in that case).
    /// Never null!
    /// </summary>
    /// <param name="dat">The TagData used to construct this BooleanTag.</param>
    /// <param name="input">The input to create or get a boolean from.</param>
    /// <returns>The boolean tag.</returns>
    public static BooleanTag For(TemplateObject input, TagData dat)
    {
        return CreateFor(input, dat);
    }

    /// <summary>Tries to return a valid boolean, or null.</summary>
    /// <param name="input">The input that is potentially a boolean.</param>
    /// <returns>A boolean, or null.</returns>
    public static BooleanTag TryFor(string input)
    {
        string low = input.ToLowerFast();
        if (low == "true")
        {
            return TRUE;
        }
        if (low == "false")
        {
            return FALSE;
        }
        return null;
    }

    /// <summary>Tries to return a valid boolean, or null.</summary>
    /// <param name="input">The input that is potentially a boolean.</param>
    /// <returns>A boolean, or null.</returns>
    public static BooleanTag TryFor(TemplateObject input)
    {
        if (input == null)
        {
            return null;
        }
        if (input is BooleanTag bt)
        {
            return bt;
        }
        return TryFor(input.ToString());
    }

    /// <summary>The BooleanTag type.</summary>
    public const string TYPE = "boolean";

    /// <summary>Creates a BooleanTag for the given input data.</summary>
    /// <param name="dat">The tag data.</param>
    /// <param name="input">The text input.</param>
    /// <returns>A valid boolean tag.</returns>
    public static BooleanTag CreateFor(TemplateObject input, TagData dat)
    {
        return input switch
        {
            BooleanTag itag => itag,
            DynamicTag dtag => CreateFor(dtag.Internal, dat),
            _ => For(dat, input.ToString()),
        };
    }

#pragma warning disable 1591

    [TagMeta(TagType = TYPE, Name = "not", Group = "Boolean Logic", ReturnType = TYPE, Returns = "The opposite of the tag - true and false are flipped.",
        Examples = ["'true' .not returns 'false'.", "'false' .not returns 'true'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Not(BooleanTag obj, TagData data)
    {
        return ForBool(!obj.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "and", Group = "Boolean Logic", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the boolean and the specified text are both true.",
        Examples = ["'true' .and[true] returns 'true'.", "'true' .and[false] returns 'false'.", "'false' .and[true] returns 'false'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_And(BooleanTag obj, BooleanTag modifier)
    {
        return ForBool(obj.Internal && modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "or", Group = "Boolean Logic", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the boolean or the specified text are true.",
        Examples = ["'true' .or[true] returns 'true'.", "'true' .or[false] returns 'true'.", "'false' .or[false] returns 'false'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Or(BooleanTag obj, BooleanTag modifier)
    {
        return ForBool(obj.Internal || modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "xor", Group = "Boolean Logic", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the boolean exclusive-or the specified text are true. Meaning, exactly one of the two must be true, and the other false.",
        Examples = ["'true' .xor[true] returns 'false'.", "'true' .xor[false] returns 'true.'"])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Xor(BooleanTag obj, BooleanTag modifier)
    {
        return ForBool(obj.Internal != modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Duplicate(BooleanTag obj, TagData data)
    {
        return ForBool(obj.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of the tag (BooleanTag).")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TagTypeTag Tag_Type(BooleanTag obj, TagData data)
    {
        return new TagTypeTag(data.TagSystem.Types.Type_Boolean);
    }

#pragma warning restore 1591

    /// <summary>Returns the a string representation of the boolean internally stored by this boolean tag. IE, this returns "true" or "false".</summary>
    /// <returns>A string representation of the boolean.</returns>
    public override string ToString()
    {
        return Internal ? "true" : "false";
    }
}
