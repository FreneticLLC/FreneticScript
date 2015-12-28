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
        // <--[object]
        // @Type TextTag
        // @SubType NONE
        // @Group Mathematics
        // @Description Represents any text.
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
                // @Example "alpha" .to_list returns "a|l|p|h|a".
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
                    return new BooleanTag(Text.ToLower() == data.GetModifier(0).ToLower()).Handle(data.Shrink());
                // <--[tag]
                // @Name TextTag.does_not_equal_ignore_case[<TextTag>]
                // @Group Text Comparison
                // @ReturnType BooleanTag
                // @Returns whether the text matches the specified text, ignoring letter casing.
                // @Example "alpha" .does_not_equal_ignore_case[ALPHA] returns "false".
                // -->
                case "does_not_equal_ignore_case":
                    return new BooleanTag(Text.ToLower() != data.GetModifier(0).ToLower()).Handle(data.Shrink());
                default:
                    break;
            }
            // TODO: Queue level error!
            data.TagSystem.CommandSystem.Output.Bad("Invalid tag bit: '" + TagParser.Escape(data.Input[0]) + "'!", data.mode);
            return "&{UNKNOWN_TAG_BIT:" + data.Input[0] + "}";
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
