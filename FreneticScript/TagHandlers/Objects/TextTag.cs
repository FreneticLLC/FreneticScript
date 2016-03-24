using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents text as a usable tag.
    /// </summary>
    public class TextTag : TemplateObject
    {
        // <--[object]
        // @Type TextTag
        // @SubType NONE
        // @Group Mathematics
        // @Description Represents any text.
        // @Other <@link explanation Text Tags>What are text tags?<@/link>
        // -->

        /// <summary>
        /// The text this TextTag represents.
        /// </summary>
        string Text = null;

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from.</param>
        public TextTag(string _text)
        {
            Text = _text;
        }
        
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
                // @Name TextTag.to_number
                // @Group Text Modification
                // @ReturnType NumberTag
                // @Returns the text parsed as a number.
                // @Example "1" .to_number returns "1".
                // -->
                case "to_number":
                    {
                        NumberTag numtag = NumberTag.For(data, Text);
                        if (numtag == null)
                        {
                            return new NullTag();
                        }
                        return numtag.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name TextTag.to_integer
                // @Group Text Modification
                // @ReturnType NumberTag
                // @Returns the text parsed as an integer.
                // @Example "1" .to_integer returns "1".
                // -->
                case "to_integer":
                    {
                        IntegerTag numtag = IntegerTag.For(data, Text);
                        if (numtag == null)
                        {
                            return new NullTag();
                        }
                        return numtag.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name TextTag.to_boolean
                // @Group Text Modification
                // @ReturnType BooleanTag
                // @Returns the text parsed as a boolean.
                // @Example "true" .to_boolean returns "true".
                // -->
                case "to_boolean":
                    {
                        BooleanTag booltag = BooleanTag.For(data, Text);
                        if (booltag == null)
                        {
                            return new NullTag();
                        }
                        return booltag.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name TextTag.is_number
                // @Group Text Modification
                // @ReturnType BooleanTag
                // @Returns whether the text represents a valid number.
                // @Example "1" .is_number returns "true".
                // -->
                case "is_number":
                    return new BooleanTag(NumberTag.For(data, Text) != null).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_integer
                // @Group Text Modification
                // @ReturnType BooleanTag
                // @Returns whether the text represents a valid integer.
                // @Example "1" .is_integer returns "true".
                // -->
                case "is_integer":
                    return new BooleanTag(IntegerTag.For(data, Text) != null).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.is_boolean
                // @Group Text Modification
                // @ReturnType BooleanTag
                // @Returns whether the text represents a valid boolean.
                // @Example "true" .is_boolean returns "true".
                // -->
                case "is_boolean":
                    return new BooleanTag(BooleanTag.For(data, Text) != null).Handle(data.Shrink());
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
                    return new TextTag(Text.ToLowerInvariant()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_list_of_characters
                // @Group Text Modification
                // @ReturnType ListTag
                // @Returns the text as a list of characters.
                // @Other can be reverted via <@link tag ListTag.unseparated>ListTag.unseparated<@/link>.
                // @Example "alpha" .to_list_of_characters returns "a|l|p|h|a".
                // -->
                case "to_list_of_characters":
                    {
                        List<TemplateObject> list = new List<TemplateObject>(Text.Length);
                        for (int i = 0; i < Text.Length; i++)
                        {
                            list.Add(new TextTag(Text[i].ToString()));
                        }
                        return new ListTag(list).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name TextTag.substring[<NumberTag>,<NumberTag>]
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
                        int num1 = (int)NumberTag.For(data, inputs[0]).Internal - 1; // TODO: Integer tag?
                        int num2 = (int)NumberTag.For(data, inputs[1]).Internal - 1;
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
                // @ReturnType NumberTag
                // @Returns the number of characters in the text.
                // @Example "alpha" .length returns "5".
                // -->
                case "length":
                    return new NumberTag(Text.Length).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.equals[<TextTag>]
                // @Group Text Comparison
                // @ReturnType BooleanTag
                // @Returns whether the text matches the specified text.
                // @Other note that this is case-sensitive.
                // @Example "alpha" .equals[alpha] returns "true".
                // -->
                case "equals":
                    return new BooleanTag(Text == data.GetModifier(0)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.does_not_equal[<TextTag>]
                // @Group Text Comparison
                // @ReturnType BooleanTag
                // @Returns whether the text does not match the specified text.
                // @Other note that this is case-sensitive.
                // @Example "alpha" .does_not_equal[alpha] returns "false".
                // -->
                case "does_not_equal":
                    return new BooleanTag(Text != data.GetModifier(0)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.equals_ignore_case[<TextTag>]
                // @Group Text Comparison
                // @ReturnType BooleanTag
                // @Returns whether the text matches the specified text, ignoring letter casing.
                // @Example "alpha" .equals_ignore_case[ALPHA] returns "true".
                // -->
                case "equals_ignore_case":
                    return new BooleanTag(Text.ToLowerInvariant() == data.GetModifier(0).ToLowerInvariant()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.does_not_equal_ignore_case[<TextTag>]
                // @Group Text Comparison
                // @ReturnType BooleanTag
                // @Returns whether the text matches the specified text, ignoring letter casing.
                // @Example "alpha" .does_not_equal_ignore_case[ALPHA] returns "false".
                // -->
                case "does_not_equal_ignore_case":
                    return new BooleanTag(Text.ToLowerInvariant() != data.GetModifier(0).ToLowerInvariant()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.to_utf8_binary
                // @Group Conversion
                // @ReturnType BinaryTag
                // @Returns UTF-8 encoded binary data of the included text.
                // @Other can be reverted via <@link tag BinaryTag.from_utf8>BinaryTag.from_utf8<@/link>.
                // @Example "hi" .to_utf8_binary returns "6869".
                // -->
                case "to_utf8_binary":
                    return new BinaryTag(new UTF8Encoding(false).GetBytes(Text)).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.from_base64
                // @Group Conversion
                // @ReturnType BinaryTag
                // @Returns the binary data represented by this Base-64 text.
                // @Other can be reverted via <@link tag BinaryTag.to_base64>BinaryTag.to_base64<@/link>.
                // @Example "aGk=" .from_base64 returns "6869".
                // -->
                case "from_base64":
                    try
                    {
                        byte[] bits = Convert.FromBase64String(Text);
                        if (bits == null)
                        {
                            data.Error("Invalid base64 input: '" + TagParser.Escape(Text) + "'!");
                            return new NullTag();
                        }
                        return new BinaryTag(bits).Handle(data.Shrink());
                    }
                    catch (FormatException ex)
                    {
                        data.Error("Invalid base64 input: '" + TagParser.Escape(Text) + "', with internal message: '" + TagParser.Escape(ex.Message) + "'!");
                        return new NullTag();
                    }
                default:
                    break;
            }
            if (!data.HasFallback)
            {
                data.Error("Invalid tag bit: '" + TagParser.Escape(data[0]) + "'!");
            }
            return new NullTag();
        }

        /// <summary>
        /// Converts the text tag to a string by returning the internal text.
        /// </summary>
        /// <returns>A string representation of this text tag.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
