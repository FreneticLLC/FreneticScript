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
using FreneticScript.UtilitySystems;

namespace FreneticScript.TagHandlers.Objects;

/// <summary>Represents a number as a usable tag.</summary>
/// <param name="_val">The internal number to use.</param>
[ObjectMeta(Name = NumberTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Mathematics", Description = "Represents a number.",
    Others = ["Note that the number is internally stored as a 64-bit signed floating point number (a 'double')."])]
public class NumberTag(double _val) : TemplateObject, INumberTagForm
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
        return tagTypeSet.Type_Number;
    }

    /// <summary>The number this NumberTag represents.</summary>
    public double Internal = _val;

    /// <summary>The number value of this NumberTag-like object.</summary>
    public double NumberForm
    {
        get
        {
            return Internal;
        }
    }

    /// <summary>Helper validator to validate an argument as a number tag.</summary>
    /// <param name="validator">The validation helper.</param>
    public static void Validator(ArgumentValidation validator)
    {
        validator.ObjectValue = For(validator.ObjectValue, validator.ErrorAction);
    }

    /// <summary>
    /// Get a number tag relevant to the specified input, erroring on the command system if invalid input is given.
    /// Never null!
    /// </summary>
    /// <param name="err">Error call if something goes wrong.</param>
    /// <param name="input">The input text to create a number from.</param>
    /// <returns>The number tag.</returns>
    public static NumberTag For(Action<string> err, string input) // TODO: Remove this method signature (in favor of TagData variant and TryFor variant).
    {
        if (double.TryParse(input, out double tval))
        {
            return new NumberTag(tval);
        }
        err("Invalid number: '" + TextStyle.Separate + input + TextStyle.Base + "'!");
        throw new Exception("Error didn't cause exception.");
    }

    /// <summary>
    /// Get a number tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
    /// Never null!
    /// </summary>
    /// <param name="err">Error call if something goes wrong.</param>
    /// <param name="input">The input text to create a number from.</param>
    /// <returns>The number tag.</returns>
    public static NumberTag For(TemplateObject input, Action<string> err)
    {
        return input switch
        {
            NumberTag ntag => ntag,
            IntegerTag itag => new NumberTag(itag.Internal),
            IIntegerTagForm itf => new NumberTag(itf.IntegerForm),
            INumberTagForm ntf => new NumberTag(ntf.NumberForm),
            DynamicTag dtag => For(dtag.Internal, err),
            _ => For(err, input.ToString()),
        };
    }

    /// <summary>
    /// Get a number tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
    /// Never null!
    /// </summary>
    /// <param name="dat">The TagData used to construct this NumberTag.</param>
    /// <param name="input">The input text to create a number from.</param>
    /// <returns>The number tag.</returns>
    public static NumberTag For(TagData dat, string input)
    {
        if (double.TryParse(input, out double tval))
        {
            return new NumberTag(tval);
        }
        throw dat.Error("Invalid number: '" + TextStyle.Separate + input + TextStyle.Base + "'!");
    }

    /// <summary>
    /// Get a number tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
    /// Never null!
    /// </summary>
    /// <param name="dat">The TagData used to construct this NumberTag.</param>
    /// <param name="input">The input text to create a number from.</param>
    /// <returns>The number tag.</returns>
    public static NumberTag For(TemplateObject input, TagData dat)
    {
        return input as NumberTag ?? For(dat, input.ToString());
    }

    /// <summary>Tries to return a valid number, or null.</summary>
    /// <param name="input">The input that is potentially a number.</param>
    /// <returns>A number, or null.</returns>
    public static NumberTag TryFor(string input)
    {
        if (double.TryParse(input, out double tval))
        {
            return new NumberTag(tval);
        }
        return null;
    }

    /// <summary>Tries to return a valid number, or null.</summary>
    /// <param name="input">The input that is potentially a number.</param>
    /// <returns>A number, or null.</returns>
    public static NumberTag TryFor(TemplateObject input)
    {
        if (input == null)
        {
            return null;
        }
        if (input is NumberTag tag)
        {
            return tag;
        }
        return TryFor(input.ToString());
    }

    /// <summary>Constructs a number tag for a given integer tag.</summary>
    /// <param name="obj">The integer tag object.</param>
    /// <returns>The number tag result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag ForIntegerTag(TemplateObject obj)
    {
        return new NumberTag((obj as IntegerTag).Internal);
    }

    /// <summary>Creates a NumberTag for the given input data.</summary>
    /// <param name="dat">The tag data.</param>
    /// <param name="input">The text input.</param>
    /// <returns>A valid number tag.</returns>
    public static NumberTag CreateFor(TemplateObject input, TagData dat)
    {
        return input switch
        {
            NumberTag ntag => ntag,
            IntegerTag itag => new NumberTag(itag.Internal),
            IIntegerTagForm itf => new NumberTag(itf.IntegerForm),
            INumberTagForm ntf => new NumberTag(ntf.NumberForm),
            DynamicTag dtag => CreateFor(dtag.Internal, dat),
            _ => For(dat, input.ToString()),
        };
    }

    /// <summary>The NumberTag type.</summary>
    public const string TYPE = "number";

#pragma warning disable 1591

    [TagMeta(TagType = TYPE, Name = "is_greater_than", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the number is greater than the specified number.",
        Examples = ["'1' .is_greater_than[0] returns 'true'."],
        Others = ["Commonly shortened to '>'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Greater_Than(NumberTag obj, NumberTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal > modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "is_greater_than_or_equal_to", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the number is greater than or equal to the specified number.",
        Examples = ["'1' .is_greater_than_or_equal_to[0] returns 'true'."],
        Others = ["Commonly shortened to '>='."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Greater_Than_Or_Equal_To(NumberTag obj, NumberTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal >= modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "is_less_than", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the number is less than the specified number.",
        Examples = ["'1' .is_less_than[0] returns 'false'."],
        Others = ["Commonly shortened to '<'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Less_Than(NumberTag obj, NumberTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal < modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "is_less_than_or_equal_to", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the number is less than or equal to the specified number.",
        Examples = ["'1' .is_less_than_or_equal_to[0] returns 'false'."],
        Others = ["Commonly shortened to '<='."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Less_Than_Or_Equal_To(NumberTag obj, NumberTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal <= modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "equals", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the number is equal to the specified number.",
        Examples = ["'1' .equals[0] returns 'false'."],
        Others = ["Commonly shortened to '=='."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Equals(NumberTag obj, NumberTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal == modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "add", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the number plus the specified number.",
        Examples = ["'1' .add[1] returns '2'."],
        Others = ["Commonly shortened to '+'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Add(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(obj.Internal + modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "subtract", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the number minus the specified number.",
        Examples = ["'1' .subtract[1] returns '0'."],
        Others = ["Commonly shortened to '-'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Subtract(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(obj.Internal - modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "multiply", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the number multiplied by the specified number.",
        Examples = ["'2' .multiply[2] returns '4'."],
        Others = ["Commonly shortened to '*'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Multiply(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(obj.Internal * modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "divide", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the number divided by the specified number.",
        Examples = ["'4' .multiply[2] returns '2'."],
        Others = ["Commonly shortened to '/'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Divide(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(obj.Internal / modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "modulo", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "Whether the number modulo the specified number.",
        Examples = ["'10' .modulo[3] returns '1'."],
        Others = ["Commonly shortened to '%'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Modulo(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(obj.Internal % modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "round", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The number rounded to the nearest whole number.",
        Examples = ["'1.4' .round returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Round(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Round(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "absolute_value", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The absolute value of the number.",
        Examples = ["'-1' .absolute_value returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Absolute_Value(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Abs(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "sine", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The sine of the number.",
        Examples = ["'3.14159' .sine returns '0'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Sine(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Sin(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "cosine", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The cosine of the number.",
        Examples = ["'3.14159' .cosine returns '-1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Cosine(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Cos(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "tangent", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The tangent of the number.",
        Examples = ["'3.14159' .sine returns '0'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Tangent(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Tan(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "arcsine", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The inverse sine of the number.",
        Examples = ["'0' .arcsine returns '0'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Arcsine(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Asin(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "arccosine", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The inverse cosine of the number.",
        Examples = ["'1' .arccosine returns '0'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Arccosine(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Acos(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "arctangent", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The inverse tangent of the number.",
        Examples = ["'0' .arctangent returns '0'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Arctangent(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Atan(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "atan2", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The inverse tangent that is the number divided by the specified number.",
        Examples = ["'0' .atan2[1] returns '0'"])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Atan2(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(Math.Atan2(obj.Internal, modifier.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "hyperbolic_sine", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The hyperbolic sine of the number.",
        Examples = ["'0' .hyperbolic_sine returns '0'"])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Hyperbolic_Sine(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Sinh(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "hyperbolic_cosine", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The hyperbolic cosine of the number.",
        Examples = ["'0' .hyperbolic_cosine returns '1'"])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Hyperbolic_Cosine(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Cosh(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "hyperbolic_tangent", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The hyperbolic tangent of the number.",
        Examples = ["'0' .hyperbolic_sine returns '0'"])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Hyperbolic_Tangent(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Tanh(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "round_up", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The number rounded to the next whole number.",
        Examples = ["'1.4' .round_up returns '2'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Round_Up(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Ceiling(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "round_down", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The number rounded to the previous whole number.",
        Examples = ["'1.6' .round_down returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Round_Down(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Floor(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "log10", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The base-10 logarithm of the number.",
        Examples = ["'10' .log10 returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Log10(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Log10(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "log", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The logarithm (to the base of the specified number) of the number.",
        Examples = ["'2' .log[2] returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Log(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(Math.Log(obj.Internal, modifier.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "maximum", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The higher (between the number and the specific number).",
        Examples = ["'10' .maximum[12] returns '12'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Maximum(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(Math.Max(obj.Internal, obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "minimum", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The lower number (between the number and the specific number).",
        Examples = ["'10' .minimum[12] returns '10'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Minimum(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(Math.Min(obj.Internal, obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "power", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The number to the power of the specified number.",
        Examples = ["'2' .power[2] returns '4'."],
        Others = ["Commonly shortened to '^'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Power(NumberTag obj, NumberTag modifier)
    {
        return new NumberTag(Math.Pow(obj.Internal, modifier.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "sign", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The sign of the number, which can be -1, 0, or 1.",
        Examples = ["'-5' .sign returns '-1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Sign(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Sign(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "square_root", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The square root of the number.",
        Examples = ["'4' .square_root returns '2'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Square_Root(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Sqrt(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "truncate", Group = "Mathematics", ReturnType = TYPE,
        Returns = "The integral part of the number",
        Examples = ["'-1.7' .truncate returns '-1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Truncate(NumberTag obj, TagData data)
    {
        return new NumberTag(Math.Truncate(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "to_binary", Group = "Number Conversion", ReturnType = BinaryTag.TYPE,
        Returns = "The binary representation of the number.",
        Examples = ["'1' .to_binary returns '0000000000000FF3'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BinaryTag Tag_To_Binary(NumberTag obj, TagData data)
    {
        return new BinaryTag(BitConverter.GetBytes(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "to_integer", Group = "Number Conversion", ReturnType = IntegerTag.TYPE,
        Returns = "The integer representation of the number.",
        Examples = ["'1.0' .to_integer returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntegerTag Tag_To_Integer(NumberTag obj, TagData data)
    {
        return new IntegerTag((long)obj.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "to_number", Group = "Number Conversion", ReturnType = TYPE,
        Returns = "The number.",
        Examples = ["'1' .to_number returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_To_Number(NumberTag obj, TagData data)
    {
        return obj;
    }

    [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_Duplicate(NumberTag obj, TagData data)
    {
        return new NumberTag(obj.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (NumberTag).")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TagTypeTag Tag_Type(NumberTag obj, TagData data)
    {
        return new TagTypeTag(data.TagSystem.Types.Type_Number);
    }

#pragma warning restore 1591

    /// <summary>Returns the a string representation of the number internally stored by this number tag. EG, could return "0", or "1", or "-1.005", or...</summary>
    /// <returns>A string representation of the number.</returns>
    public override string ToString()
    {
        return Internal.ToString();
    }

    /// <summary>Adds a value to the object.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to add.</param>
    [ObjectOperationAttribute(ObjectOperation.ADD, Input = TYPE)]
    public static NumberTag Add(NumberTag first, NumberTag val)
    {
        return new NumberTag(first.Internal + val.Internal);
    }

    /// <summary>Subtracts a value from the object.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to subtract.</param>
    [ObjectOperationAttribute(ObjectOperation.SUBTRACT, Input = TYPE)]
    public static NumberTag Subtract(NumberTag first, NumberTag val)
    {
        return new NumberTag(first.Internal - val.Internal);
    }

    /// <summary>Multiplies the object by a value.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to multiply by.</param>
    [ObjectOperationAttribute(ObjectOperation.MULTIPLY, Input = TYPE)]
    public static NumberTag Multiply(NumberTag first, NumberTag val)
    {
        return new NumberTag(first.Internal * val.Internal);
    }

    /// <summary>Divides the object by a value.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to divide by.</param>
    [ObjectOperationAttribute(ObjectOperation.DIVIDE, Input = TYPE)]
    public static NumberTag Divide(NumberTag first, NumberTag val)
    {
        return new NumberTag(first.Internal / val.Internal);
    }
}
