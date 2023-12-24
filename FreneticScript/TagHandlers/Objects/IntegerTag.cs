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
using FreneticScript.CommandSystem;

namespace FreneticScript.TagHandlers.Objects;

/// <summary>Represents an integer number as a usable tag.</summary>
/// <param name="_val">The internal integer to use.</param>
[ObjectMeta(Name = IntegerTag.TYPE, SubTypeName = NumberTag.TYPE, RawInternal = true, Group = "Mathematics", Description = "Represents an integer.",
    Others = ["Note that the number is internally stored as a 64-bit signed integer (a 'long')."])]
public class IntegerTag(long _val) : TemplateObject, IIntegerTagForm, INumberTagForm
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
        return tagTypeSet.Type_Integer;
    }

    /// <summary>The integer this IntegerTag represents.</summary>
    public long Internal = _val;

    /// <summary>The integer value of this IntegerTag-like object.</summary>
    public long IntegerForm => Internal;

    /// <summary>The number value of this NumberTag-like object.</summary>
    public double NumberForm => Internal;

    /// <summary>Helper validator to validate an argument as an integer tag.</summary>
    /// <param name="validator">The validation helper.</param>
    public static void Validator(ArgumentValidation validator)
    {
        validator.ObjectValue = For(validator.ObjectValue, validator.ErrorAction);
    }

    /// <summary>
    /// Get an integer tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
    /// Never null!
    /// </summary>
    /// <param name="err">Error call if something goes wrong.</param>
    /// <param name="input">The input text to create a integer from.</param>
    /// <returns>The integer tag.</returns>
    public static IntegerTag For(Action<string> err, string input)
    {
        if (long.TryParse(input, out long tval))
        {
            return new IntegerTag(tval);
        }
        err($"Invalid integer: '{input}'!");
        return new IntegerTag(0);
    }

    /// <summary>
    /// Get an integer tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
    /// Never null!
    /// </summary>
    /// <param name="err">Error call if something goes wrong.</param>
    /// <param name="input">The input text to create a integer from.</param>
    /// <returns>The integer tag.</returns>
    public static IntegerTag For(TemplateObject input, Action<string> err)
    {
        return input switch
        {
            IntegerTag itag => itag,
            IIntegerTagForm itf => new IntegerTag(itf.IntegerForm),
            DynamicTag dtag => For(dtag.Internal, err),
            _ => For(err, input.ToString()),
        };
    }

    /// <summary>
    /// Get an integer tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
    /// Never null!
    /// </summary>
    /// <param name="dat">The TagData used to construct this IntegerTag.</param>
    /// <param name="input">The input text to create a integer from.</param>
    /// <returns>The integer tag.</returns>
    public static IntegerTag For(TagData dat, string input)
    {
        if (long.TryParse(input, out long tval))
        {
            return new IntegerTag(tval);
        }
        throw dat.Error("Invalid integer: '" + input + "'!");
    }

    /// <summary>
    /// Get an integer tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
    /// Never null!
    /// </summary>
    /// <param name="dat">The TagData used to construct this IntegerTag.</param>
    /// <param name="input">The input text to create a integer from.</param>
    /// <returns>The integer tag.</returns>
    public static IntegerTag For(TemplateObject input, TagData dat)
    {
        return input as IntegerTag ?? For(dat, input.ToString());
    }

    /// <summary>Tries to return a valid integer, or null.</summary>
    /// <param name="input">The input that is potentially an integer.</param>
    /// <returns>An integer, or null.</returns>
    public static IntegerTag TryFor(string input)
    {
        if (long.TryParse(input, out long tval))
        {
            return new IntegerTag(tval);
        }
        return null;
    }

    /// <summary>Tries to return a valid integer, or null.</summary>
    /// <param name="input">The input that is potentially an integer.</param>
    /// <returns>An integer, or null.</returns>
    public static IntegerTag TryFor(TemplateObject input)
    {
        if (input == null)
        {
            return null;
        }
        if (input is IntegerTag it)
        {
            return it;
        }
        return TryFor(input.ToString());
    }

    /// <summary>The IntegerTag type.</summary>
    public const string TYPE = "integer";

    /// <summary>Creates an IntegerTag for the given input data.</summary>
    /// <param name="dat">The tag data.</param>
    /// <param name="input">The text input.</param>
    /// <returns>A valid integer tag.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CreateFor_Raw(TemplateObject input, TagData dat)
    {
        switch (input)
        {
            case IntegerTag itag:
                return itag.Internal;
            case IIntegerTagForm itf:
                return itf.IntegerForm;
            case DynamicTag dtag:
                return CreateFor_Raw(dtag.Internal, dat);
            default:
                if (long.TryParse(input.ToString(), out long tval))
                {
                    return tval;
                }
                throw dat.Error($"Invalid integer: '{input}'!");
        }
    }

    /// <summary>Creates an IntegerTag for the given input data.</summary>
    /// <param name="dat">The tag data.</param>
    /// <param name="input">The text input.</param>
    /// <returns>A valid integer tag.</returns>
    public static IntegerTag CreateFor(TemplateObject input, TagData dat)
    {
        return input switch
        {
            IntegerTag itag => itag,
            IIntegerTagForm itf => new IntegerTag(itf.IntegerForm),
            DynamicTag dtag => CreateFor(dtag.Internal, dat),
            _ => For(dat, input.ToString()),
        };
    }

#pragma warning disable 1591

    [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntegerTag Tag_Duplicate(IntegerTag obj, TagData data)
    {
        return new IntegerTag(obj.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (IntegerTag).")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TagTypeTag Tag_Type(IntegerTag obj, TagData data)
    {
        return new TagTypeTag(data.TagSystem.Types.Type_Integer);
    }

    [TagMeta(TagType = TYPE, Name = "add_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The integer plus the specified integer.",
        Examples = ["'1' .add_int[1] returns '2'."], Others = ["Commonly shortened to '+'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Add_Int(long obj, long modifier)
    {
        return obj + modifier;
    }

    [TagMeta(TagType = TYPE, Name = "subtract_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The integer minus the specified integer.",
        Examples = ["'2' .subtract_int[1] returns '1'."], Others = ["Commonly shortened to '-'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Subtract_Int(long obj, long modifier)
    {
        return obj - modifier;
    }

    [TagMeta(TagType = TYPE, Name = "multiply_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The integer multiplied by the specified integer.",
        Examples = ["'2' .multiply_int[2] returns '4'."], Others = ["Commonly shortened to '*'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Multiply_Int(long obj, long modifier)
    {
        return obj * modifier;
    }

    [TagMeta(TagType = TYPE, Name = "divide_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The integer divided by the specified integer.",
        Examples = ["'4' .divide_int[2] returns '2'."], Others = ["Commonly shortened to '/'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Divide_Int(long obj, long modifier)
    {
        return obj / modifier;
    }

    [TagMeta(TagType = TYPE, Name = "modulo_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The integer modulo (remainder after division) the specified integer.",
        Examples = ["'10' .modulo_int[3] returns '1'."], Others = ["Commonly shortened to '%'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Modulo_Int(long obj, long modifier)
    {
        return obj % modifier;
    }

    [TagMeta(TagType = TYPE, Name = "absolute_value_int", Group = "Mathematics", ReturnType = TYPE, Returns = "The absolute value of the integer.",
        Examples = ["'-1' .absolute_value_int returns '1'."], Others = ["Commonly shortened to 'abs'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Absolute_Value_Int(long obj, TagData data)
    {
        return Math.Abs(obj);
    }

    [TagMeta(TagType = TYPE, Name = "maximum_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whichever is greater: the integer or the specified integer.",
        Examples = ["'10' .maximum_int[12] returns '12'."], Others = ["Commonly shortened to 'max'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Maximum_Int(long obj, long modifier)
    {
        return Math.Max(obj, modifier);
    }

    [TagMeta(TagType = TYPE, Name = "minimum_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whichever is lower: the integer or the specified integer.",
        Examples = ["'10' .minimum_int[12] returns '10'."], Others = ["Commonly shortened to 'min'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Minimum_Int(long obj, long modifier)
    {
        return Math.Min(obj, modifier);
    }

    [TagMeta(TagType = TYPE, Name = "to_binary", Group = "Conversion", ReturnType = BinaryTag.TYPE, Returns = "a binary representation of this integer.",
        Examples = ["'1' .to_binary returns '1000000000000000'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BinaryTag Tag_To_Binary(long obj, TagData data)
    {
        return new BinaryTag(BitConverter.GetBytes(obj));
    }

    [TagMeta(TagType = TYPE, Name = "is_greater_than", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the integer is greater than the specified integer.",
        Examples = ["'1' .is_greater_than[0] returns 'true'."], Others = ["Commonly shortened to '>'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Greater_Than(long obj, long modifier)
    {
        return BooleanTag.ForBool(obj > modifier);
    }

    [TagMeta(TagType = TYPE, Name = "is_greater_than_or_equal_to", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the integer is greater than or equal to the specified integer.",
        Examples = ["'1' .is_greater_than_or_equal_to[0] returns 'true'."], Others = ["Commonly shortened to '>='."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Greater_Than_Or_Equal_To(long obj, long modifier)
    {
        return BooleanTag.ForBool(obj >= modifier);
    }

    [TagMeta(TagType = TYPE, Name = "is_less_than", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the integer is less than the specified integer.",
        Examples = ["'1' .is_less_than[0] returns 'false'."], Others = ["Commonly shortened to '<'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Less_Than(long obj, long modifier)
    {
        return BooleanTag.ForBool(obj < modifier);
    }

    [TagMeta(TagType = TYPE, Name = "is_less_than_or_equal_to", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the integer is less than or equal to the specified integer.",
        Examples = ["'1' .is_less_than_or_equal_to[0] returns 'false'."], Others = ["Commonly shortened to '<='."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Less_Than_Or_Equal_To(long obj, long modifier)
    {
        return BooleanTag.ForBool(obj <= modifier);
    }

    [TagMeta(TagType = TYPE, Name = "equals", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the integer equals the specified integer.",
        Examples = ["'1' .equals[0] returns 'false'."], Others = ["Commonly shortened to '=='."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Equals(long obj, long modifier)
    {
        return BooleanTag.ForBool(obj == modifier);
    }

    [TagMeta(TagType = TYPE, Name = "sign", Group = "Mathematics", ReturnType = TYPE, Returns = "The sign of the integer, which can be -1, 0, or 1.",
        Examples = ["'-5' .sign returns '-1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_Sign(long obj, TagData data)
    {
        return Math.Sign(obj);
    }

    [TagMeta(TagType = TYPE, Name = "to_integer", Group = "Conversion", ReturnType = TYPE, Returns = "The integer parsed as an integer.",
        Examples = ["'1' .to_integer returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Tag_To_Integer(long obj, TagData data)
    {
        return obj;
    }

    [TagMeta(TagType = TYPE, Name = "to_number", Group = "Conversion", ReturnType = NumberTag.TYPE, Returns = "The integer parsed as a number.",
        Examples = ["'1' .to_number returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_To_Number(long obj, TagData data)
    {
        return new NumberTag(obj);
    }

#pragma warning restore 1591

    /// <summary>Returns the a string representation of the integer internally stored by this integer tag. EG, could return "0", or "1", or...</summary>
    /// <returns>A string representation of the integer.</returns>
    public override string ToString()
    {
        return Internal.ToString();
    }

    /// <summary>Adds a value to the object.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to add.</param>
    [ObjectOperationAttribute(ObjectOperation.ADD, Input = TYPE)]
    public static IntegerTag Add(IntegerTag first, IntegerTag val)
    {
        return new IntegerTag(first.Internal + val.Internal);
    }

    /// <summary>Subtracts a value from the object.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to subtract.</param>
    [ObjectOperationAttribute(ObjectOperation.SUBTRACT, Input = TYPE)]
    public static IntegerTag Subtract(IntegerTag first, IntegerTag val)
    {
        return new IntegerTag(first.Internal - val.Internal);
    }

    /// <summary>Multiplies the object by a value.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to multiply by.</param>
    [ObjectOperationAttribute(ObjectOperation.MULTIPLY, Input = TYPE)]
    public static IntegerTag Multiply(IntegerTag first, IntegerTag val)
    {
        return new IntegerTag(first.Internal * val.Internal);
    }

    /// <summary>Divides the object by a value.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to divide by.</param>
    [ObjectOperationAttribute(ObjectOperation.DIVIDE, Input = TYPE)]
    public static IntegerTag Divide(IntegerTag first, IntegerTag val)
    {
        return new IntegerTag(first.Internal / val.Internal);
    }
}
