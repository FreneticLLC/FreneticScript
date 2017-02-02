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
        string Internal = null;

        /// <summary>
        /// Constructs a text tag.
        /// </summary>
        /// <param name="_text">The text to construct it from.</param>
        public TextTag(string _text)
        {
            Internal = _text;
        }

        /// <summary>
        /// Converts a template object to a text tag.
        /// </summary>
        /// <param name="text">The text input.</param>
        /// <returns>A valid text tag.</returns>
        public static TextTag For(TemplateObject text)
        {
            return new TextTag(text.ToString());
        }

        /// <summary>
        /// Creates a TextTag for the given input data.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <param name="text">The text input.</param>
        /// <returns>A valid text tag.</returns>
        public static TemplateObject CreateFor(TagData data, TemplateObject text)
        {
            return new TextTag(text.ToString());
        }

        /// <summary>
        /// The TextTag type.
        /// </summary>
        public const string TYPE = "texttag";

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.",
            Examples = new string[] { "'Hello' .duplicate returns 'Hello'." })]
        public static TemplateObject Tag_Duplicate(TagData data, TemplateObject obj)
        {
            return new TextTag(obj.ToString());
        }

#pragma warning restore 1591

        /// <summary>
        /// All tag handlers for this tag type.
        /// </summary>
        public static Dictionary<string, TagSubHandler> Handlers = new Dictionary<string, TagSubHandler>();

        static TextTag()
        {
            // Needs docs!
            Handlers.Add("type", new TagSubHandler() { Handle = (data, obj) => new TagTypeTag(data.TagSystem.Type_Text), ReturnTypeString = "tagtypetag" });
            // Needs docs!
            Handlers.Add("or_else", new TagSubHandler() { Handle = (data, obj) => obj, ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.to_number
            // @Group Text Modification
            // @ReturnType NumberTag
            // @Returns the text parsed as a number.
            // @Example "1" .to_number returns "1".
            // -->
            Handlers.Add("to_number", new TagSubHandler() { Handle = (data, obj) => NumberTag.For(data, obj), ReturnTypeString = "numbertag" });
            // <--[tag]
            // @Name TextTag.to_integer
            // @Group Text Modification
            // @ReturnType IntegerTag
            // @Returns the text parsed as an integer.
            // @Example "1" .to_integer returns "1".
            // -->
            Handlers.Add("to_integer", new TagSubHandler() { Handle = (data, obj) => IntegerTag.For(data, obj), ReturnTypeString = "integertag" });
            // <--[tag]
            // @Name TextTag.to_boolean
            // @Group Text Modification
            // @ReturnType BooleanTag
            // @Returns the text parsed as a boolean.
            // @Example "true" .to_boolean returns "true".
            // -->
            Handlers.Add("to_boolean", new TagSubHandler() { Handle = (data, obj) => BooleanTag.For(data, obj), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.is_number
            // @Group Text Modification
            // @ReturnType BooleanTag
            // @Returns whether the text represents a valid number.
            // @Example "1" .is_number returns "true".
            // -->
            Handlers.Add("is_number", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(NumberTag.TryFor(obj) != null), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.is_integer
            // @Group Text Modification
            // @ReturnType BooleanTag
            // @Returns whether the text represents a valid integer.
            // @Example "1" .is_integer returns "true".
            // -->
            Handlers.Add("is_integer", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(IntegerTag.TryFor(obj) != null), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.is_boolean
            // @Group Text Modification
            // @ReturnType BooleanTag
            // @Returns whether the text represents a valid boolean.
            // @Example "true" .is_boolean returns "true".
            // -->
            Handlers.Add("is_boolean", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(BooleanTag.TryFor(obj) != null), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.to_upper
            // @Group Text Modification
            // @ReturnType TextTag
            // @Returns the text in full upper-case.
            // @Example "alpha" .to_upper returns "ALPHA".
            // -->
            Handlers.Add("to_upper", new TagSubHandler() { Handle = (data, obj) => new TextTag(obj.ToString().ToUpperInvariant()), ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.to_lower
            // @Group Text Modification
            // @ReturnType TextTag
            // @Returns the text in full lower-case.
            // @Example "ALPHA" .to_lower returns "alpha".
            // -->
            Handlers.Add("to_lower", new TagSubHandler() { Handle = (data, obj) => new TextTag(obj.ToString().ToLowerFast()), ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.to_list_of_characters
            // @Group Text Modification
            // @ReturnType ListTag
            // @Returns the text as a list of characters.
            // @Other Can be reverted via <@link tag ListTag.unseparated>ListTag.unseparated<@/link>.
            // @Example "alpha" .to_list_of_characters returns "a|l|p|h|a".
            // -->
            Handlers.Add("to_list_of_characters", new TagSubHandler() { Handle = (data, obj) =>
            {
                string Text = obj.ToString();
                List<TemplateObject> list = new List<TemplateObject>(Text.Length);
                for (int i = 0; i < Text.Length; i++)
                {
                    list.Add(new TextTag(Text[i].ToString()));
                }
                return new ListTag(list).Handle(data.Shrink());
            }, ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.replace[<ListTag>]
            // @Group Text Modification
            // @ReturnType TextTag
            // @Returns the text with all instances of the first text replaced with the second.
            // @Example "alpha" .replace[a|b] returns "blphb".
            // -->
            Handlers.Add("replace", new TagSubHandler() { Handle = (data, obj) =>
                {
                    ListTag modif = ListTag.For(data.GetModifierObject(0));
                    if (modif.ListEntries.Count != 2)
                    {
                        data.Error("Invalid replace tag! Not two entries in the list!");
                        return new NullTag();
                    }
                    return new TextTag(obj.ToString().Replace(modif.ListEntries[0].ToString(), modif.ListEntries[1].ToString()));
                },  ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.substring[<ListTag>]
            // @Group Text Modification
            // @ReturnType TextTag
            // @Returns the portion of text in the specified range.
            // @Other Note that indices are one-based.
            // @Example "alpha" .substring[2|4] returns "lph".
            // @Example "alpha" .substring[2|99999] will return "lpha".
            // -->
            Handlers.Add("substring", new TagSubHandler() { Handle = (data, obj) =>
            {
                string Text = obj.ToString();
                ListTag inputs = ListTag.For(data.GetModifierObject(0));
                if (inputs.ListEntries.Count < 2)
                {
                    data.Error("Invalid substring tag! Not two entries in the list!");
                    return new NullTag();
                }
                int num1 = (int)IntegerTag.For(data, inputs.ListEntries[0]).Internal - 1;
                int num2 = (int)IntegerTag.For(data, inputs.ListEntries[1]).Internal - 1;
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
                return new TextTag(Text.Substring(num1, (num2 - num1) + 1));
            },  ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.append[<TextTag>]
            // @Group Text Modification
            // @ReturnType TextTag
            // @Returns the text with the input text appended.
            // @Example "alpha" .append[bet] returns "alphabet".
            // -->
            Handlers.Add("append", new TagSubHandler() { Handle = (data, obj) => new TextTag(obj.ToString() + data.GetModifier(0)), ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.prepend[<TextTag>]
            // @Group Text Modification
            // @ReturnType TextTag
            // @Returns the text with the input text prepended.
            // @Example "alpha" .prepend[bet] returns "betalpha".
            // -->
            Handlers.Add("prepend", new TagSubHandler() { Handle = (data, obj) => new TextTag(data.GetModifier(0) + obj.ToString()), ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name TextTag.length
            // @Group Text Attributes
            // @ReturnType NumberTag
            // @Returns the number of characters in the text.
            // @Example "alpha" .length returns "5".
            // -->
            Handlers.Add("length", new TagSubHandler() { Handle = (data, obj) => new IntegerTag(obj.ToString().Length), ReturnTypeString = "integertag" });
            // <--[tag]
            // @Name TextTag.equals[<TextTag>]
            // @Group Text Comparison
            // @ReturnType BooleanTag
            // @Returns whether the text matches the specified text.
            // @Other Note that this is case-sensitive.
            // @Example "alpha" .equals[alpha] returns "true".
            // -->
            Handlers.Add("equals", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(obj.ToString() == data.GetModifier(0)), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.does_not_equal[<TextTag>]
            // @Group Text Comparison
            // @ReturnType BooleanTag
            // @Returns whether the text does not match the specified text.
            // @Other Note that this is case-sensitive.
            // @Example "alpha" .does_not_equal[alpha] returns "false".
            // -->
            Handlers.Add("does_not_equal", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(obj.ToString() != data.GetModifier(0)), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.equals_ignore_case[<TextTag>]
            // @Group Text Comparison
            // @ReturnType BooleanTag
            // @Returns whether the text matches the specified text, ignoring letter casing.
            // @Example "alpha" .equals_ignore_case[ALPHA] returns "true".
            // -->
            Handlers.Add("equals_ignore_case", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(obj.ToString().ToLowerFast() == data.GetModifier(0).ToLowerFast()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.does_not_equal_ignore_case[<TextTag>]
            // @Group Text Comparison
            // @ReturnType BooleanTag
            // @Returns whether the text matches the specified text, ignoring letter casing.
            // @Example "alpha" .does_not_equal_ignore_case[ALPHA] returns "false".
            // -->
            Handlers.Add("dos_not_equal_ignore_case", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(obj.ToString().ToLowerFast() != data.GetModifier(0).ToLowerFast()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.contains[<TextTag>]
            // @Group Text Comparison
            // @ReturnType BooleanTag
            // @Returns whether the text contains the specified text.
            // @Other Note that this is case-sensitive.
            // @Example "alpha" .contains[alp] returns "true".
            // -->
            Handlers.Add("contains", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(obj.ToString().Contains(data.GetModifier(0))), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name TextTag.to_utf8_binary
            // @Group Conversion
            // @ReturnType BinaryTag
            // @Returns UTF-8 encoded binary data of the included text.
            // @Other Can be reverted via <@link tag BinaryTag.from_utf8>BinaryTag.from_utf8<@/link>.
            // @Example "hi" .to_utf8_binary returns "8696".
            // -->
            Handlers.Add("to_utf8_binary", new TagSubHandler() { Handle = (data, obj) => new BinaryTag(new UTF8Encoding(false).GetBytes(obj.ToString())), ReturnTypeString = "binarytag" });
            // <--[tag]
            // @Name TextTag.from_base64
            // @Group Conversion
            // @ReturnType BinaryTag
            // @Returns the binary data represented by this Base-64 text.
            // @Other Can be reverted via <@link tag BinaryTag.to_base64>BinaryTag.to_base64<@/link>.
            // @Example "aGk=" .from_base64 returns "6869".
            // -->
            Handlers.Add("from_base64", new TagSubHandler() { Handle = (data, obj) =>
            {
                string Text = obj.ToString();
                try
                {
                    byte[] bits = Convert.FromBase64String(Text);
                    if (bits == null)
                    {
                        data.Error("Invalid base64 input: '" + TagParser.Escape(Text) + "'!");
                        return new NullTag();
                    }
                    return new BinaryTag(bits);
                }
                catch (FormatException ex)
                {
                    data.Error("Invalid base64 input: '" + TagParser.Escape(Text) + "', with internal message: '" + TagParser.Escape(ex.Message) + "'!");
                    return new NullTag();
                }
            },  ReturnTypeString = "texttag" });
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
            TagSubHandler handler;
            if (Handlers.TryGetValue(data[0], out handler))
            {
                return handler.Handle(data, this).Handle(data.Shrink());
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
            return Internal;
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
                Internal = val.ToString();
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
                Internal += val.ToString();
                return;
            }
            base.Add(names, val);
        }
    }
}
