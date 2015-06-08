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
                // @Example "alpha" .to_upper returns "ALPHA".
                // -->
                case "to_upper":
                    return new TextTag(Text.ToUpper()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_lower
                // @Group Text Modification
                // @ReturnType TextTag
                // @Returns the text in full lower-case.
                // @Example "ALPHA" .to_lower returns "alpha".
                // -->
                case "to_lower":
                    return new TextTag(Text.ToLower()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_list
                // @Group Text Modification
                // @ReturnType ListTag
                // @Returns the text as a list of characters.
                // @Other can be reverted via <@link tag ListTag.unseparated>ListTag.unseparated<@/link>.
                // @Example "alpha" .to_upper returns "a|l|p|h|a".
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
                // @Other note that indices are one-based.
                // @Example "alpha" .substring[2,4] returns "lph".
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
                // @Name TextTag.append[<TextTag>]
                // @Group Text Modification
                // @ReturnType TextTag
                // @Returns the text with the input text appended.
                // @Example "alpha" .append[bet] returns "alphabet".
                // -->
                case "append":
                    return new TextTag(Text + data.GetModifier(0)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.prepend[<TextTag>]
                // @Group Text Modification
                // @ReturnType TextTag
                // @Returns the text with the input text prepended.
                // @Example "alpha" .prepend[bet] returns "betalpha".
                // -->
                case "prepend":
                    return new TextTag(data.GetModifier(0) + Text).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.length
                // @Group Text Attributes
                // @ReturnType TextTag
                // @Returns the number of characters in the text.
                // @Example "alpha" .length returns "5".
                // -->
                case "length":
                    return new TextTag(Text.Length).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.not
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns the opposite of the tag - true and false are flipped.
                // @Example "true" .not returns "false".
                // -->
                case "not":
                    return new TextTag(Text.ToLower() == "false").Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.equals[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text matches the specified text.
                // @Other note that this is case-sensitive.
                // @Example "alpha" .equals[alpha] returns "true".
                // -->
                case "equals":
                    return new TextTag(Text == data.GetModifier(0)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.does_not_equal[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text does not match the specified text.
                // @Other note that this is case-sensitive.
                // @Example "alpha" .does_not_equal[alpha] returns "false".
                // -->
                case "does_not_equal":
                    return new TextTag(Text != data.GetModifier(0)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.equals_ignore_case[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text matches the specified text, ignoring letter casing.
                // @Example "alpha" .equals_ignore_case[ALPHA] returns "true".
                // -->
                case "equals_ignore_case":
                    return new TextTag(Text.ToLower() == data.GetModifier(0).ToLower()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.does_not_equal_ignore_case[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text matches the specified text, ignoring letter casing.
                // @Example "alpha" .does_not_equal_ignore_case[ALPHA] returns "false".
                // -->
                case "does_not_equal_ignore_case":
                    return new TextTag(Text.ToLower() != data.GetModifier(0).ToLower()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.and[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text and the specified text are both 'true'.
                // @Example "true" .and[true] returns "true".
                // -->
                case "and":
                    return new TextTag(Text.ToLower() == "true" && data.GetModifier(0).ToLower() == "true").Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.or[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text or the specified text are 'true'.
                // @Example "true" .or[false] returns "true".
                // -->
                case "or":
                    return new TextTag(Text.ToLower() == "true" || data.GetModifier(0).ToLower() == "true").Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.xor[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TextTag
                // @Returns whether the text exclusive or the specified text are 'true'.
                // @Examplre "true" .xor[true] returns "false".
                // -->
                case "xor":
                    return new TextTag((Text.ToLower() == "true") != (data.GetModifier(0).ToLower() == "true")).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_greater_than[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is greater than the specified number.
                // @Other commonly shortened to ">".
                // @Example "1" .is_greater_than[0] returns "true".
                // -->
                case "is_greater_than":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) >=
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_greater_than_or_equal_to[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is greater than or equal to the specified number.
                // @Other Commonly shortened to ">=".
                // @Example "1" .is_greater_than_or_equal_to[0] returns "true".
                // -->
                case "is_greater_than_or_equal_to":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) >=
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_less_than[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is less than to the specified number.
                // @Other commonly shortened to "<".
                // @Example "1" .is_less_than[0] returns "false".
                // -->
                case "is_less_than":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) <
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_less_than_or_equal_to[<TextTag>]
                // @Group Number Comparison
                // @ReturnType TextTag
                // @Returns whether the number is less than or equal to the specified number.
                // @Other Commonly shortened to "<=".
                // @Example "1" .is_less_than_or_equal_to[0] returns "false".
                // -->
                case "is_less_than_or_equal_to":
                    return new TextTag(FreneticUtilities.StringToDouble(Text) <=
                        FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.add[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number plus the specified number.
                // @Other Commonly shortened to "+".
                // @Example "1" .add[1] returns "2".
                // -->
                case "add":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        + FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.subtract[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number minus the specified number.
                // @Other Commonly shortened to "-".
                // @Example "1" .subtract[1] returns "0".
                // -->
                case "subtract":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        - FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.multiply[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number multiplied by the specified number.
                // @Other Commonly shortened to "*".
                // @Example "2" .multiply[2] returns "4".
                // -->
                case "multiply":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        * FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.divide[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number divided by the specified number.
                // @Other Commonly shortened to "/".
                // @Example "4" .divide[2] returns "2".
                // -->
                case "divide":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        / FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.modulo[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number modulo the specified number.
                // @Other Commonly shortened to "%".
                // @Example "10" .modulo[3] returns "1".
                // -->
                case "modulo":
                    return new TextTag(FreneticUtilities.StringToDouble(Text)
                        % FreneticUtilities.StringToDouble(data.GetModifier(0))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.round
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number without any decimals.
                // @Example "1.4" .round returns "1".
                // -->
                case "round":
                    return new TextTag(Math.Round(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.absolute_value
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the absolute value of the number.
                // @Example "-1" .absolute_value returns "1".
                // -->
                case "absolute_value":
                    return new TextTag(Math.Abs(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.cosine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the cosine of the number.
                // @Example "3.14159" .cosine returns "-1".
                // -->
                case "cosine":
                    return new TextTag(Math.Cos(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.sine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the sine of the number.
                // @Example "3.14159" .sine returns "0".
                // -->
                case "sine":
                    return new TextTag(Math.Sin(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.arccosine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse cosine of the number.
                // @Example "1" .arccosine returns "0".
                // -->
                case "arccosine":
                    return new TextTag(Math.Acos(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.arcsine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse sin of the number.
                // @Example "0" .arcsine returns "0".
                // -->
                case "arcsine":
                    return new TextTag(Math.Asin(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.arctangent
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse tangent of the number.
                // @Example "0" .arctangent returns "0".
                // -->
                case "arctangent":
                    return new TextTag(Math.Atan(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.tangent
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the tangent of the number.
                // @Example "3.14159" .tangent returns "0".
                // -->
                case "tangent":
                    return new TextTag(Math.Tan(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.atan2[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the inverse of the tangent that is the text divided by the specified text.
                // @Example "0" .atan2[1] returns "0".
                // -->
                case "atan2":
                    return new TextTag(Math.Atan2(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.round_up
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number rounded up.
                // @Example "1.4" .round_up returns "2".
                // -->
                case "round_up":
                    return new TextTag(Math.Ceiling(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.round_down
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number rounded down.
                // @Example "1.6" .round_down returns "1".
                // -->
                case "round_down":
                    return new TextTag(Math.Floor(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.log10
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the base-10 logarithm of the number.
                // @Example "10" .log10 returns "1".
                // -->
                case "log10":
                    return new TextTag(Math.Log10(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.log[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the logarithm(base: specified number) of the number.
                // @Example "2" .log[2] returns "1".
                // -->
                case "log":
                    return new TextTag(Math.Log(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.maximum[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns whichever is greater: the number or the specified number.
                // @Example "10" .maximum[12] returns "12".
                // -->
                case "maximum":
                    return new TextTag(Math.Max(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.minimum[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns whichever is lower: the number or the specified number.
                // @Example "10" .minimum[12] returns "10".
                // -->
                case "minimum":
                    return new TextTag(Math.Min(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_the_power_of[<TextTag>]
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the number to the power of the specified number.
                // @Example "2" .to_the_power_of[2] returns "4".
                // -->
                case "to_the_power_of":
                    return new TextTag(Math.Pow(FreneticUtilities.StringToDouble(Text),
                        FreneticUtilities.StringToDouble(data.GetModifier(0)))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.sign
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the sign of the number, which can be -1, 0, or 1.
                // @Example "-5" .sign returns "-1".
                // -->
                case "sign":
                    return new TextTag(Math.Sign(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.hyperbolic_sine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the hyperbolic sine of the number.
                // @Example "0" .hyperbolic_sine returns "0".
                // -->
                case "hyperbolic_sine":
                    return new TextTag(Math.Sinh(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.hyperbolic_cosine
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the hyperbolic cosine of the number.
                // @Example "0" .hyperbolic_cosine returns "1".
                // -->
                case "hyperbolic_cosine":
                    return new TextTag(Math.Cosh(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.hyperbolic_tangent
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the hyperbolic tangent of the number.
                // @Example "0" .hyperbolic_tangent returns "0".
                // -->
                case "hyperbolic_tangent":
                    return new TextTag(Math.Tanh(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.square_root
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the square root of the number.
                // @Example "4" .square_root returns "2".
                // -->
                case "square_root":
                    return new TextTag(Math.Sqrt(FreneticUtilities.StringToDouble(Text))).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.truncate
                // @Group Mathematics
                // @ReturnType TextTag
                // @Returns the truncated version of the number - essentially, rounding towards zero.
                // @Example "-1.7" .hyperbolic_sine returns "-1".
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
