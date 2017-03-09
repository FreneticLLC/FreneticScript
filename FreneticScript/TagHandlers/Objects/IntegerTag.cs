using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a number as a usable tag.
    /// </summary>
    public class IntegerTag : TemplateObject
    {
        // <--[object]
        // @Type IntegerTag
        // @SubType NumberTag
        // @Group Mathematics
        // @Description Represents an integer.
        // @Other note that the number is internally stored as a 64-bit signed integer (a 'long').
        // -->

        /// <summary>
        /// The integer this IntegerTag represents.
        /// </summary>
        public long Internal;

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
            if (!dat.HasFallback)
            {
                dat.Error("Invalid integer: '" + TagParser.Escape(input) + "'!");
            }
            return new IntegerTag(0);
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

        /// <summary>
        /// Tries to return a valid integer, or null.
        /// </summary>
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

        /// <summary>
        /// Tries to return a valid integer, or null.
        /// </summary>
        /// <param name="input">The input that is potentially an integer.</param>
        /// <returns>An integer, or null.</returns>
        public static IntegerTag TryFor(TemplateObject input)
        {
            if (input == null)
            {
                return null;
            }
            if (input is IntegerTag)
            {
                return (IntegerTag)input;
            }
            return TryFor(input.ToString());
        }

        /// <summary>
        /// Constructs an integer tag.
        /// </summary>
        /// <param name="_val">The internal integer to use.</param>
        public IntegerTag(long _val)
        {
            Internal = _val;
        }

        /// <summary>
        /// The IntegerTag type.
        /// </summary>
        public const string TYPE = "integertag";
        
        /// <summary>
        /// Creates an IntegerTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid integer tag.</returns>
        public static IntegerTag CreateFor(TemplateObject input, TagData dat)
        {
            switch (input)
            {
                case IntegerTag itag:
                    return itag;
                case DynamicTag dtag:
                    return CreateFor(dtag.Internal, dat);
                default:
                    return For(dat, input.ToString());
            }
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Duplicate(IntegerTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal);
        }
        
        [TagMeta(TagType = TYPE, Name = "add_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
            Returns = "The integer plus the specified integer.",
            Examples = new string[] { "'1' .add_int[1] returns '2'." }, Others = new string[] { "Commonly shortened to '+'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Add_Int(IntegerTag obj, IntegerTag modifier)
        {
            return new IntegerTag(obj.Internal + modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "subtract_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
            Returns = "The integer minus the specified integer.",
            Examples = new string[] { "'2' .subtract_int[1] returns '1'." }, Others = new string[] { "Commonly shortened to '-'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Subtract_Int(IntegerTag obj, IntegerTag modifier)
        {
            return new IntegerTag(obj.Internal - modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "multiply_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
            Returns = "The integer multiplied by the specified integer.",
            Examples = new string[] { "'2' .multiply_int[2] returns '4'." }, Others = new string[] { "Commonly shortened to '*'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Multiply_Int(IntegerTag obj, IntegerTag modifier)
        {
            return new IntegerTag(obj.Internal * modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "divide_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
            Returns = "The integer divided by the specified integer.",
            Examples = new string[] { "'4' .divide_int[2] returns '2'." }, Others = new string[] { "Commonly shortened to '/'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Divide_Int(IntegerTag obj, IntegerTag modifier)
        {
            return new IntegerTag(obj.Internal / modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "modulo_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
            Returns = "The integer modulo (remainder after division) the specified integer.",
            Examples = new string[] { "'10' .modulo_int[3] returns '1'." }, Others = new string[] { "Commonly shortened to '%'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Modulo_Int(IntegerTag obj, IntegerTag modifier)
        {
            return new IntegerTag(obj.Internal % modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "absolute_value_int", Group = "Mathematics", ReturnType = TYPE, Returns = "The absolute value of the integer.",
            Examples = new string[] { "'-1' .absolute_value_int returns '1'." }, Others = new string[] { "Commonly shortened to 'abs'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Absolute_Value_Int(IntegerTag obj, TagData data)
        {
            return new IntegerTag(Math.Abs(obj.Internal));
        }

        [TagMeta(TagType = TYPE, Name = "maximum_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
            Returns = "Whichever is greater: the integer or the specified integer.",
            Examples = new string[] { "'10' .maximum_int[12] returns '12'." }, Others = new string[] { "Commonly shortened to 'max'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Maximum_Int(IntegerTag obj, IntegerTag modifier)
        {
            return new IntegerTag(Math.Max(obj.Internal, modifier.Internal));
        }

        [TagMeta(TagType = TYPE, Name = "minimum_int", Group = "Mathematics", ReturnType = TYPE, Modifier = TYPE,
            Returns = "Whichever is lower: the integer or the specified integer.",
            Examples = new string[] { "'10' .minimum_int[12] returns '10'." }, Others = new string[] { "Commonly shortened to 'min'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Minimum_Int(IntegerTag obj, IntegerTag modifier)
        {
            return new IntegerTag(Math.Min(obj.Internal, modifier.Internal));
        }

        [TagMeta(TagType = TYPE, Name = "to_binary", Group = "Conversion", ReturnType = BinaryTag.TYPE, Returns = "a binary representation of this integer.",
            Examples = new string[] { "'1' .to_binary returns '1000000000000000'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BinaryTag Tag_To_Binary(IntegerTag obj, TagData data)
        {
            return new BinaryTag(BitConverter.GetBytes(obj.Internal));
        }

        [TagMeta(TagType = TYPE, Name = "is_greater_than", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
            Returns = "Whether the integer is greater than the specified integer.",
            Examples = new string[] { "'1' .is_greater_than[0] returns 'true'." }, Others = new string[] { "Commonly shortened to '>'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BooleanTag Tag_Is_Greater_Than(IntegerTag obj, IntegerTag modifier)
        {
            return new BooleanTag(obj.Internal > modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "is_greater_than_or_equal_to", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
            Returns = "Whether the integer is greater than or equal to the specified integer.",
            Examples = new string[] { "'1' .is_greater_than_or_equal_to[0] returns 'true'." }, Others = new string[] { "Commonly shortened to '>='." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BooleanTag Tag_Is_Greater_Than_Or_Equal_To(IntegerTag obj, IntegerTag modifier)
        {
            return new BooleanTag(obj.Internal >= modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "is_less_than", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
            Returns = "Whether the integer is less than the specified integer.",
            Examples = new string[] { "'1' .is_less_than[0] returns 'false'." }, Others = new string[] { "Commonly shortened to '<'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BooleanTag Tag_Is_Less_Than(IntegerTag obj, IntegerTag modifier)
        {
            return new BooleanTag(obj.Internal < modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "is_less_than_or_equal_to", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
            Returns = "Whether the integer is less than or equal to the specified integer.",
            Examples = new string[] { "'1' .is_less_than_or_equal_to[0] returns 'false'." }, Others = new string[] { "Commonly shortened to '<='." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BooleanTag Tag_Is_Less_Than_Or_Equal_To(IntegerTag obj, IntegerTag modifier)
        {
            return new BooleanTag(obj.Internal <= modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "equals", Group = "Number Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
            Returns = "Whether the integer equals the specified integer.",
            Examples = new string[] { "'1' .equals[0] returns 'false'." }, Others = new string[] { "Commonly shortened to '=='." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BooleanTag Tag_Equals(IntegerTag obj, IntegerTag modifier)
        {
            return new BooleanTag(obj.Internal == modifier.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "sign", Group = "Mathematics", ReturnType = TYPE, Returns = "The sign of the integer, which can be -1, 0, or 1.",
            Examples = new string[] { "'-5' .sign returns '-1'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Sign(IntegerTag obj, TagData data)
        {
            return new IntegerTag(Math.Sign(obj.Internal));
        }

        [TagMeta(TagType = TYPE, Name = "to_integer", Group = "Conversion", ReturnType = TYPE, Returns = "The integer parsed as an integer.",
            Examples = new string[] { "'1' .to_integer returns '1'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_To_Integer(IntegerTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "to_number", Group = "Conversion", ReturnType = NumberTag.TYPE, Returns = "The integer parsed as a number.",
            Examples = new string[] { "'1' .to_number returns '1'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NumberTag Tag_To_Number(IntegerTag obj, TagData data)
        {
            return new NumberTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (IntegerTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(IntegerTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Type_Integer);
        }

#pragma warning restore 1591
        
        /// <summary>
        /// Returns the a string representation of the integer internally stored by this integer tag. EG, could return "0", or "1", or...
        /// </summary>
        /// <returns>A string representation of the integer.</returns>
        public override string ToString()
        {
            return Internal.ToString();
        }

        /// <summary>
        /// Sets a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to set it to.</param>
        public override void Set(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal = TryFor(val).Internal;
                return;
            }
            base.Set(names, val);
        }

        /// <summary>
        /// Adds a value to a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to add.</param>
        public override void Add(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal += TryFor(val).Internal;
                return;
            }
            base.Add(names, val);
        }

        /// <summary>
        /// Subtracts a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to subtract.</param>
        public override void Subtract(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal -= TryFor(val).Internal;
                return;
            }
            base.Subtract(names, val);
        }

        /// <summary>
        /// Multiplies a value by a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to multiply.</param>
        public override void Multiply(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal *= TryFor(val).Internal;
                return;
            }
            base.Multiply(names, val);
        }

        /// <summary>
        /// Divides a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to divide.</param>
        public override void Divide(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal /= TryFor(val).Internal;
                return;
            }
            base.Divide(names, val);
        }
    }
}
