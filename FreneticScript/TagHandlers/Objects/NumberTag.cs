using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a number as a usable tag.
    /// </summary>
    public class NumberTag : TemplateObject
    {
        // <--[object]
        // @Type NumberTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents a number.
        // @Other note that the number is internally stored as a 64-bit signed floating point number (a 'double').
        // -->

        /// <summary>
        /// The number this NumberTag represents.
        /// </summary>
        public double Internal;
        
        /// <summary>
        /// Get a number tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
        /// Never null!
        /// </summary>
        /// <param name="dat">The TagData used to construct this NumberTag.</param>
        /// <param name="input">The input text to create a number from.</param>
        /// <returns>The number tag.</returns>
        public static NumberTag For(TagData dat, string input)
        {
            double tval;
            if (double.TryParse(input, out tval))
            {
                return new NumberTag(tval);
            }
            if (!dat.HasFallback)
            {
                dat.Error("Invalid number: '" + TagParser.Escape(input) + "'!");
            }
            return new NumberTag(0);
        }

        /// <summary>
        /// Get a number tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
        /// Never null!
        /// </summary>
        /// <param name="dat">The TagData used to construct this NumberTag.</param>
        /// <param name="input">The input text to create a number from.</param>
        /// <returns>The number tag.</returns>
        public static NumberTag For(TagData dat, TemplateObject input)
        {
            return input as NumberTag ?? For(dat, input.ToString());
        }

        /// <summary>
        /// Tries to return a valid number, or null.
        /// </summary>
        /// <param name="input">The input that is potentially a number.</param>
        /// <returns>A number, or null.</returns>
        public static NumberTag TryFor(string input)
        {
            double tval;
            if (double.TryParse(input, out tval))
            {
                return new NumberTag(tval);
            }
            return null;
        }

        /// <summary>
        /// Tries to return a valid number, or null.
        /// </summary>
        /// <param name="input">The input that is potentially a number.</param>
        /// <returns>A number, or null.</returns>
        public static NumberTag TryFor(TemplateObject input)
        {
            if (input == null)
            {
                return null;
            }
            if (input is NumberTag)
            {
                return (NumberTag)input;
            }
            return TryFor(input.ToString());
        }

        /// <summary>
        /// Constructs a number tag.
        /// </summary>
        /// <param name="_val">The internal number to use.</param>
        public NumberTag(double _val)
        {
            Internal = _val;
        }

        /// <summary>
        /// The NumberTag type.
        /// </summary>
        public const string TYPE = "numbertag";

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override TemplateObject Handle(TagData data)
        {
            if (data.Remaining == 0)
            {
                return this;
            }
            switch (data[0])
            {
                // <--[tag]
                // @Name NumberTag.is_greater_than[<NumberTag>]
                // @Group Number Comparison
                // @ReturnType BooleanTag
                // @Returns whether the number is greater than the specified number.
                // @Other commonly shortened to ">".
                // @Example "1" .is_greater_than[0] returns "true".
                // -->
                case "is_greater_than":
                    return new BooleanTag(Internal >= For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.is_greater_than_or_equal_to[<NumberTag>]
                // @Group Number Comparison
                // @ReturnType BooleanTag
                // @Returns whether the number is greater than or equal to the specified number.
                // @Other Commonly shortened to ">=".
                // @Example "1" .is_greater_than_or_equal_to[0] returns "true".
                // -->
                case "is_greater_than_or_equal_to":
                    return new BooleanTag(Internal >= For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.is_less_than[<NumberTag>]
                // @Group Number Comparison
                // @ReturnType BooleanTag
                // @Returns whether the number is less than to the specified number.
                // @Other commonly shortened to "<".
                // @Example "1" .is_less_than[0] returns "false".
                // -->
                case "is_less_than":
                    return new BooleanTag(Internal < For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                    // Documented in TextTag.
                case "equals":
                    return new BooleanTag(Internal == For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.is_less_than_or_equal_to[<NumberTag>]
                // @Group Number Comparison
                // @ReturnType BooleanTag
                // @Returns whether the number is less than or equal to the specified number.
                // @Other Commonly shortened to "<=".
                // @Example "1" .is_less_than_or_equal_to[0] returns "false".
                // -->
                case "is_less_than_or_equal_to":
                    return new BooleanTag(Internal <= For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.add[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number plus the specified number.
                // @Other Commonly shortened to "+".
                // @Example "1" .add[1] returns "2".
                // -->
                case "add":
                    return new NumberTag(Internal + For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.subtract[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number minus the specified number.
                // @Other Commonly shortened to "-".
                // @Example "1" .subtract[1] returns "0".
                // -->
                case "subtract":
                    return new NumberTag(Internal - For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.multiply[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number multiplied by the specified number.
                // @Other Commonly shortened to "*".
                // @Example "2" .multiply[2] returns "4".
                // -->
                case "multiply":
                    return new NumberTag(Internal * For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.divide[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number divided by the specified number.
                // @Other Commonly shortened to "/".
                // @Example "4" .divide[2] returns "2".
                // -->
                case "divide":
                    return new NumberTag(Internal / For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.modulo[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number modulo the specified number.
                // @Other Commonly shortened to "%".
                // @Example "10" .modulo[3] returns "1".
                // -->
                case "modulo":
                    return new NumberTag(Internal % For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.round
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number without any decimals.
                // @Example "1.4" .round returns "1".
                // -->
                case "round":
                    return new NumberTag(Math.Round(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.absolute_value
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the absolute value of the number.
                // @Example "-1" .absolute_value returns "1".
                // -->
                case "absolute_value":
                    return new NumberTag(Math.Abs(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.cosine
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the cosine of the number.
                // @Example "3.14159" .cosine returns "-1".
                // -->
                case "cosine":
                    return new NumberTag(Math.Cos(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.sine
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the sine of the number.
                // @Example "3.14159" .sine returns "0".
                // -->
                case "sine":
                    return new NumberTag(Math.Sin(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.arccosine
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the inverse cosine of the number.
                // @Example "1" .arccosine returns "0".
                // -->
                case "arccosine":
                    return new NumberTag(Math.Acos(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.arcsine
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the inverse sin of the number.
                // @Example "0" .arcsine returns "0".
                // -->
                case "arcsine":
                    return new NumberTag(Math.Asin(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.arctangent
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the inverse tangent of the number.
                // @Example "0" .arctangent returns "0".
                // -->
                case "arctangent":
                    return new NumberTag(Math.Atan(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.tangent
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the tangent of the number.
                // @Example "3.14159" .tangent returns "0".
                // -->
                case "tangent":
                    return new NumberTag(Math.Tan(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.atan2[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the inverse of the tangent that is the number divided by the specified number.
                // @Example "0" .atan2[1] returns "0".
                // -->
                case "atan2":
                    return new NumberTag(Math.Atan2(Internal, For(data, data.GetModifierObject(0)).Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.round_up
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number rounded up.
                // @Example "1.4" .round_up returns "2".
                // -->
                case "round_up":
                    return new NumberTag(Math.Ceiling(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.round_down
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number rounded down.
                // @Example "1.6" .round_down returns "1".
                // -->
                case "round_down":
                    return new NumberTag(Math.Floor(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.log10
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the base-10 logarithm of the number.
                // @Example "10" .log10 returns "1".
                // -->
                case "log10":
                    return new NumberTag(Math.Log10(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.log[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the logarithm(base: specified number) of the number.
                // @Example "2" .log[2] returns "1".
                // -->
                case "log":
                    return new NumberTag(Math.Log(Internal, For(data, data.GetModifierObject(0)).Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.maximum[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns whichever is greater: the number or the specified number.
                // @Example "10" .maximum[12] returns "12".
                // -->
                case "maximum":
                    return new NumberTag(Math.Max(Internal, For(data, data.GetModifierObject(0)).Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.minimum[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns whichever is lower: the number or the specified number.
                // @Example "10" .minimum[12] returns "10".
                // -->
                case "minimum":
                    return new NumberTag(Math.Min(Internal, For(data, data.GetModifierObject(0)).Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.to_the_power_of[<NumberTag>]
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the number to the power of the specified number.
                // @Example "2" .to_the_power_of[2] returns "4".
                // -->
                case "power":
                    return new NumberTag(Math.Pow(Internal, For(data, data.GetModifierObject(0)).Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.sign
                // @Group Mathematics
                // @ReturnType IntegerTag
                // @Returns the sign of the number, which can be -1, 0, or 1.
                // @Example "-5" .sign returns "-1".
                // -->
                case "sign":
                    return new IntegerTag(Math.Sign(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.hyperbolic_sine
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the hyperbolic sine of the number.
                // @Example "0" .hyperbolic_sine returns "0".
                // -->
                case "hyperbolic_sine":
                    return new NumberTag(Math.Sinh(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.hyperbolic_cosine
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the hyperbolic cosine of the number.
                // @Example "0" .hyperbolic_cosine returns "1".
                // -->
                case "hyperbolic_cosine":
                    return new NumberTag(Math.Cosh(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.hyperbolic_tangent
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the hyperbolic tangent of the number.
                // @Example "0" .hyperbolic_tangent returns "0".
                // -->
                case "hyperbolic_tangent":
                    return new NumberTag(Math.Tanh(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.square_root
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the square root of the number.
                // @Example "4" .square_root returns "2".
                // -->
                case "square_root":
                    return new NumberTag(Math.Sqrt(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.truncate
                // @Group Mathematics
                // @ReturnType NumberTag
                // @Returns the truncated version of the number - essentially, rounding towards zero.
                // @Example "-1.7" .truncate returns "-1".
                // -->
                case "truncate":
                    return new NumberTag(Math.Truncate(Internal)).Handle(data.Shrink());
                // <--[tag]
                // @Name NumberTag.to_binary
                // @Group Mathematics
                // @ReturnType BinaryTag
                // @Returns a binary representation of this floating-point number.
                // @Example "1" .to_binary returns "0000000000000FF3".
                // -->
                case "to_binary":
                    return new BinaryTag(BitConverter.GetBytes(Internal)).Handle(data.Shrink());
                // Documented in TextTag.
                case "to_integer":
                    return new IntegerTag((long)Internal).Handle(data.Shrink());
                // Documented in TextTag.
                case "to_number":
                    return Handle(data.Shrink());
                // Documented in TextTag.
                case "duplicate":
                    return new NumberTag(Internal).Handle(data.Shrink());
                // Documented in TextTag.
                case "type":
                    return new TagTypeTag(data.TagSystem.Type_Number).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        /// <summary>
        /// Returns the a string representation of the number internally stored by this number tag. EG, could return "0", or "1", or "-1.005", or...
        /// </summary>
        /// <returns>A string representation of the number.</returns>
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
