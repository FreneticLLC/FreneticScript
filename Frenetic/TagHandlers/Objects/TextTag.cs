using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers.Objects
{
    /// <summary>
    /// Represents text as a usable tag.
    /// </summary>
    public class TextTag : TemplateObject
    {
        /// <summary>
        /// The text this TextTag represents.
        /// </summary>
        string Text = null;

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from</param>
        public TextTag(string _text)
        {
            Text = _text;
        }

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from</param>
        public TextTag(bool _text)
        {
            Text = _text ? "true" : "false";
        }

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from</param>
        public TextTag(int _text)
        {
            Text = _text.ToString();
        }

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from</param>
        public TextTag(long _text)
        {
            Text = _text.ToString();
        }

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from</param>
        public TextTag(float _text)
        {
            Text = _text.ToString();
        }

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from</param>
        public TextTag(double _text)
        {
            Text = _text.ToString();
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data</param>
        public override string Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name TextTag.to_upper
                // @Group Text Modification
                // @ReturnType TextTag
                // @Returns the text in full upper-case.
                // -->
                case "to_upper":
                    return new TextTag(Text.ToUpper()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_lower
                // @Group Text Modification
                // @ReturnType TextTag
                // @Returns the text in full lower-case.
                // -->
                case "to_lower":
                    return new TextTag(Text.ToLower()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_list
                // @Group Text Modification
                // @ReturnType ListTag
                // @Returns the text as a list of characters.
                // Can be reverted via <@link tag ListTag.unseparated>ListTag.unseparated<@/link>.
                // -->
                case "to_list":
                    {
                        List<TemplateObject> list = new List<TemplateObject>(Text.Length);
                        for (int i = 0; i < Text.Length; i++)
                        {
                            list.Add(new TextTag(Text[i].ToString()));
                        }
                        return new ListTag(list).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name TextTag.substring[<TextTag>,<TextTag>]
                // @Group Text Modification
                // @ReturnType TextTag
                // @Returns the portion of text in the specified range.
                // Note that indices are one-based.
                // EG, "hello" .substring [2,4] returns "ell".
                // -->
                case "substring":
                    {
                        string modif = data.GetModifier(0);
                        string[] inputs = modif.Split(',');
                        if (inputs.Length < 2)
                        {
                            break;
                        }
                        int num1 = FreneticUtilities.StringToInt(inputs[0]) - 1;
                        int num2 = FreneticUtilities.StringToInt(inputs[1]) - 1;
                        if (num1 < 0)
                        {
                            num1 = 0;
                        }
                        if (num1 > Text.Length - 1)
                        {
                            num1 = Text.Length - 1;
                        }
                        if (num2 < 0)
                        {
                            num2 = 0;
                        }
                        if (num2 > Text.Length - 1)
                        {
                            num2 = Text.Length - 1;
                        }
                        if (num2 < num1)
                        {
                            return new TextTag("").Handle(data.Shrink());
                        }
                        return new TextTag(Text.Substring(num1, (num2 - num1) + 1)).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name TextTag.length
                // @Group Text Attributes
                // @ReturnType TextTag
                // @Returns the number of characters in the text.
                // -->
                case "length":
                    return new TextTag(Text.Length).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.not
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns the opposite of the tag - true and false are flipped.
                // -->
                case "not":
                    return new TextTag(Text.ToLower() == "false").Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.equals[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text matches the specified text.
                // -->
                case "equals":
                    return new TextTag(Text == data.GetModifier(0)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.does_not_equal[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text does not match the specified text.
                // -->
                case "does_not_equal":
                    return new TextTag(Text != data.GetModifier(0)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.and[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text and the specified text are both 'true'.
                // -->
                case "and":
                    return new TextTag(Text.ToLower() == "true" && data.GetModifier(0).ToLower() == "true").Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.or[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text or the specified text are 'true'.
                // -->
                case "or":
                    return new TextTag(Text.ToLower() == "true" || data.GetModifier(0).ToLower() == "true").Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.xor[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text exclusive or the specified text are 'true'.
                // EG, "true" and "true" fails but "true" and "false" passes.
                // -->
                case "xor":
                    return new TextTag((Text.ToLower() == "true") != (data.GetModifier(0).ToLower() == "true")).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.equals_ignore_case[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text matches the specified text, ignoring letter casing.
                // EG, "Hello" and "hElLo" passes.
                // -->
                case "equals_ignore_case":
                    return new TextTag(Text.ToLower() == data.GetModifier(0).ToLower()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_greater_than[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is greater than the specified number.
                // Commonly shortened to ">"
                // -->
                case "is_greater_than":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) >=
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_greater_than_or_equal_to[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is greater than or equal to the specified number.
                // Commonly shortened to ">="
                // -->
                case "is_greater_than_or_equal_to":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) >=
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_less_than[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is less than to the specified number.
                // Commonly shortened to "<"
                // -->
                case "is_less_than":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) <
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_less_than_or_equal_to[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is less than or equal to the specified number.
                // Commonly shortened to "<="
                // -->
                case "is_less_than_or_equal_to":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) <=
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.add[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number plus the specified number.
                // Commonly shortened to "+"
                // -->
                case "add":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        + FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.subtract[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number minus the specified number.
                // Commonly shortened to "-"
                // -->
                case "subtract":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        - FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.multiply[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number multiplied by the specified number.
                // Commonly shortened to "*"
                // -->
                case "multiply":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        * FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.divide[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number divided by the specified number.
                // Commonly shortened to "/"
                // -->
                case "divide":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        / FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.modulo[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number modulo the specified number.
                // Commonly shortened to "%"
                // -->
                case "modulo":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        % FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.round
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number without any decimals.
                // -->
                case "round":
                    return new TextTag(Math.Round(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the absolute value of the number.
                // -->
                case "absolute_value":
                    return new TextTag(Math.Abs(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the cosine of the number.
                // -->
                case "cosine":
                    return new TextTag(Math.Cos(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the sine of the number.
                // -->
                case "sine":
                    return new TextTag(Math.Sin(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse cosine of the number.
                // -->
                case "arccosine":
                    return new TextTag(Math.Acos(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse sin of the number.
                // -->
                case "arcsine":
                    return new TextTag(Math.Asin(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse tangent of the number.
                // -->
                case "arctangent":
                    return new TextTag(Math.Atan(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the tangent of the number.
                // -->
                case "tangent":
                    return new TextTag(Math.Tan(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse of the tangent that is the text divided by the specified text.
                // -->
                case "atan2":
                    return new TextTag(Math.Atan2(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.round_up
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number rounded up.
                // -->
                case "round_up":
                    return new TextTag(Math.Ceiling(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.round_down
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number rounded down.
                // -->
                case "round_down":
                    return new TextTag(Math.Floor(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.log10
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the logarithm of the number.
                // -->
                case "log10":
                    return new TextTag(Math.Log10(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.log[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the logarithm(base: specified number) of the number.
                // -->
                case "log":
                    return new TextTag(Math.Log(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.maximum[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns whichever is greater: the number or the specified number.
                // -->
                case "maximum":
                    return new TextTag(Math.Max(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.minimum[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns whichever is lower: the number or the specified number.
                // -->
                case "minimum":
                    return new TextTag(Math.Min(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_the_power_of[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number to the power of the specified number.
                // -->
                case "to_the_power_of":
                    return new TextTag(Math.Pow(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.sign
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the sign of the number, which can be -1, 0, or 1.
                // -->
                case "sign":
                    return new TextTag(Math.Sign(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.hyperbolic_sine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the hyperbolic sine of the number.
                // -->
                case "hyperbolic_sine":
                    return new TextTag(Math.Sinh(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.hyperbolic_cosine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the hyperbolic cosine of the number.
                // -->
                case "hyperbolic_cosine":
                    return new TextTag(Math.Cosh(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.hyperbolic_tangent
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the hyperbolic tangent of the number.
                // -->
                case "hyperbolic_tangent":
                    return new TextTag(Math.Tanh(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.square_root
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the square root of the number.
                // -->
                case "square_root":
                    return new TextTag(Math.Sqrt(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.truncate
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the truncated version of the number - essentially, rounding towards zero.
                // -->
                case "truncate":
                    return new TextTag(Math.Truncate(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                default:
                    break;
            }
            return "&{UNKNOWN_TAG_BIT:" + data.Input[0] + "}";
        }

        /// <summary>
        /// Converts the text tag to a string by returning the internal text.
        /// </summary>
        /// <returns>A string representation of this text tag</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
