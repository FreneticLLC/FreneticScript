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

/// <summary>Represents text as a usable tag.</summary>
/// <param name="_text">The text to construct it from.</param>
[ObjectMeta(Name = TextTag.TYPE, SubTypeName = null, Group = "", Description = "Represents any text.",
    Others = ["<@link explanation Text Tags>What are text tags?<@/link>"])] // Not sure about "<@link>" format.
public class TextTag(string _text) : TemplateObject
{
    /// <summary>An empty text tag.</summary>
    public static readonly TextTag EMPTY = new("");

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
        return tagTypeSet.Type_Text;
    }

    /// <summary>The text this TextTag represents.</summary>
    public string Internal = _text;

    /// <summary>Helper validator to validate an argument as a text tag.</summary>
    /// <param name="validator">The validation helper.</param>
    public static void Validator(ArgumentValidation validator)
    {
        validator.ObjectValue = new TextTag(validator.ObjectValue.ToString());
    }

    /// <summary>Converts a template object to a text tag.</summary>
    /// <param name="text">The text input.</param>
    /// <returns>A valid text tag.</returns>
    public static TextTag For(TemplateObject text)
    {
        return new TextTag(text.ToString());
    }

    /// <summary>Creates a TextTag for the given input data.</summary>
    /// <param name="data">The tag data.</param>
    /// <param name="text">The text input.</param>
    /// <returns>A valid text tag.</returns>
    public static TextTag CreateFor(TemplateObject text, TagData data)
    {
        return new TextTag(text.ToString());
    }

    /// <summary>The TextTag type.</summary>
    public const string TYPE = "text";

#pragma warning disable 1591

    [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TextTag Tag_Duplicate(TextTag obj, TagData data)
    {
        return new TextTag(obj.ToString());
    }

    [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (TextTag).")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TagTypeTag Tag_Type(TextTag obj, TagData data)
    {
        return new TagTypeTag(data.TagSystem.Types.Type_Text);
    }

    [TagMeta(TagType = TYPE, Name = "to_number", Group = "Conversion", ReturnType = NumberTag.TYPE, Returns = "The text parsed as a number.",
        Examples = ["'1' .to_number returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NumberTag Tag_To_Number(TextTag obj, TagData data)
    {
        return NumberTag.For(obj, data);
    }

    [TagMeta(TagType = TYPE, Name = "to_integer", Group = "Conversion", ReturnType = IntegerTag.TYPE, Returns = "The text parsed as an integer.",
        Examples = ["'1' .to_integer returns '1'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntegerTag Tag_To_Integer(TextTag obj, TagData data)
    {
        return IntegerTag.For(obj, data);
    }

    [TagMeta(TagType = TYPE, Name = "to_boolean", Group = "Conversion", ReturnType = BooleanTag.TYPE, Returns = "The text parsed as a boolean.",
        Examples = ["'true' .to_boolean returns 'true'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_To_Boolean(TextTag obj, TagData data)
    {
        return BooleanTag.For(obj, data);
    }

    [TagMeta(TagType = TYPE, Name = "is_number", Group = "Conversion", ReturnType = BooleanTag.TYPE, Returns = "Whether the text represents a number.",
        Examples = ["'1' .is_number returns 'true'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Number(TextTag obj, TagData data)
    {
        return BooleanTag.ForBool(NumberTag.TryFor(obj) != null);
    }

    [TagMeta(TagType = TYPE, Name = "is_integer", Group = "Conversion", ReturnType = BooleanTag.TYPE, Returns = "Whether the text represents an integer.",
        Examples = ["'1' .is_integer returns 'true'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Integer(TextTag obj, TagData data)
    {
        return BooleanTag.ForBool(IntegerTag.TryFor(obj) != null);
    }

    [TagMeta(TagType = TYPE, Name = "is_boolean", Group = "Conversion", ReturnType = BooleanTag.TYPE, Returns = "Whether the text represents a boolean.",
        Examples = ["'true' .is_boolean returns 'true'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Is_Boolean(TextTag obj, TagData data)
    {
        return BooleanTag.ForBool(BooleanTag.TryFor(obj) != null);
    }

    [TagMeta(TagType = TYPE, Name = "to_upper", Group = "Text Modification", ReturnType = TYPE, Returns = "The text in full upper-case.",
        Examples = ["'alpha' .to_upper returns 'ALPHA'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TextTag Tag_To_Upper(TextTag obj, TagData data)
    {
        return new TextTag(obj.Internal.ToUpperInvariant());
    }

    [TagMeta(TagType = TYPE, Name = "to_lower", Group = "Text Modification", ReturnType = TYPE, Returns = "The text in full lower-case.",
        Examples = ["'ALPHA' .to_lower returns 'alpha'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TextTag Tag_To_Lower(TextTag obj, TagData data)
    {
        return new TextTag(obj.Internal.ToLowerFast());
    }

    [TagMeta(TagType = TYPE, Name = "to_list_of_characters", Group = "Text Modification", ReturnType = ListTag.TYPE,
        Returns = "The text as a list of characters.", Examples = ["'alpha' .to_list_of_characters returns 'a|l|p|h|a'."],
        Others = ["Can be reverted via <@link tag ListTag.unseparated>ListTag.unseparated<@/link>."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ListTag Tag_To_List_Of_Characters(TextTag obj, TagData data)
    {
        string text = obj.Internal;
        List<TemplateObject> list = new(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            list.Add(new TextTag(text[i].ToString()));
        }
        return new ListTag(list);
    }

    [TagMeta(TagType = TYPE, Name = "replace", Group = "Text Modification", ReturnType = TextTag.TYPE,
        Returns = "The text with all instances of the first text replaced with the second.",
        Examples = ["'alpha' .replace[a|b] returns 'blphb'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TextTag Tag_Replace(TextTag obj, TagData data)
    {
        ListTag modif = ListTag.CreateFor(data.GetModifierObjectCurrent());
        if (modif.Internal.Count != 2)
        {
            data.Error("Invalid replace tag! Not two entries in the list!");
            return null;
        }
        return new TextTag(obj.Internal.Replace(modif.Internal[0].ToString(), modif.Internal[1].ToString()));
    }

    [TagMeta(TagType = TYPE, Name = "substring", Group = "Text Modification", ReturnType = TextTag.TYPE,
        Returns = "The portion of text in the specified range.",
        Examples = ["'alpha' .substring[2|4] returns 'lph'.", "'alpha' .substring[2|99999] will return 'lpha'."],
        Others = ["Note that indices are one-based (This means the first entry is at index 1)."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TextTag Tag_Substring(TextTag obj, TagData data)
    {
        string text = obj.Internal;
        ListTag inputs = ListTag.CreateFor(data.GetModifierObjectCurrent(), data);
        if (inputs.Internal.Count < 2)
        {
            data.Error("Invalid substring tag! Not two entries in the list!");
            return null;
        }
        int num1 = (int)IntegerTag.For(inputs.Internal[0], data).Internal - 1;
        int num2 = (int)IntegerTag.For(inputs.Internal[1], data).Internal - 1;
        if (num1 < 0)
        {
            num1 = 0;
        }
        if (num1 > text.Length - 1)
        {
            num1 = text.Length - 1;
        }
        if (num2 < 0)
        {
            num2 = 0;
        }
        if (num2 > text.Length - 1)
        {
            num2 = text.Length - 1;
        }
        if (num2 < num1)
        {
            return new TextTag("");
        }
        return new TextTag(text.Substring(num1, (num2 - num1) + 1));
    }

    [TagMeta(TagType = TYPE, Name = "append", Group = "Text Modification", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The text with the input text appended.", Examples = ["'alpha' .append[bet] returns 'alphabet'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TextTag Tag_Append(TextTag obj, TextTag modifier)
    {
        return new TextTag(obj.Internal + modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "prepend", Group = "Text Modification", ReturnType = TYPE, Modifier = TYPE,
        Returns = "The text with the input text prepended.", Examples = ["'alpha' .prepend[bet] returns 'betalpha'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TextTag Tag_Prepend(TextTag obj, TextTag modifier)
    {
        return new TextTag(modifier.Internal + obj.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "length", Group = "Text Attributes", ReturnType = IntegerTag.TYPE, Returns = "The number of characters in the text.",
        Examples = ["'alpha' .length returns '5'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntegerTag Tag_Length(TextTag obj, TagData data)
    {
        return new IntegerTag(obj.Internal.Length);
    }

    [TagMeta(TagType = TYPE, Name = "equals", Group = "Text Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the text matches the specified text.", Examples = ["'alpha' .equals[alpha] returns 'true'."],
        Others = ["Note that this is case-sensitive."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Equals(TextTag obj, TextTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal == modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "does_not_equal", Group = "Text Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the text does not match the specified text.", Examples = ["'alpha' .does_not_equal[alpha] returns 'false'."],
        Others = ["Note that this is case-sensitive."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Does_Not_Equal(TextTag obj, TextTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal != modifier.Internal);
    }

    [TagMeta(TagType = TYPE, Name = "equals_ignore_case", Group = "Text Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the text matches the specified text, ignoring letter casing.",
        Examples = ["'alpha' .equals_ignore_case[ALPHA] returns 'true'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Equals_Ignore_Case(TextTag obj, TextTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal.ToLowerFast() == modifier.Internal.ToLowerFast());
    }

    [TagMeta(TagType = TYPE, Name = "does_not_equal_ignore_case", Group = "Text Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the text does not match the specified text, ignoring letter casing.",
        Examples = ["'alpha' .does_not_equal_ignore_case[ALPHA] returns 'false'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Does_Not_Equal_Ignore_Case(TextTag obj, TextTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal.ToLowerFast() != modifier.Internal.ToLowerFast());
    }

    [TagMeta(TagType = TYPE, Name = "contains", Group = "Text Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the text contains the specified text.", Examples = ["'alpha' .contains[alp] returns 'true'."],
        Others = ["Note that this is case-sensitive."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Contains(TextTag obj, TextTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal.Contains(modifier.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "contains_ignore_case", Group = "Text Comparison", ReturnType = BooleanTag.TYPE, Modifier = TYPE,
        Returns = "Whether the text contains the specified text, ignoring letter casing.",
        Examples = ["'alpha' .contains_ignore_case[ALP] returns 'true'."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BooleanTag Tag_Contains_Ignore_Case(TextTag obj, TextTag modifier)
    {
        return BooleanTag.ForBool(obj.Internal.ToLowerFast().Contains(modifier.Internal.ToLowerFast()));
    }

    [TagMeta(TagType = TYPE, Name = "to_utf8_binary", Group = "Conversion", ReturnType = BinaryTag.TYPE,
        Returns = "UTF-8 encoded binary data of the included text.", Examples = ["'hi' .to_utf8_binary returns '8696'."],
        Others = ["Can be reverted via <@link tag BinaryTag.from_utf8>BinaryTag.from_utf8<@/link>."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BinaryTag Tag_To_UTF8_Binary(TextTag obj, TagData data)
    {
        return new BinaryTag(new UTF8Encoding(false).GetBytes(obj.Internal));
    }

    [TagMeta(TagType = TYPE, Name = "from_base64", Group = "Conversion", ReturnType = BinaryTag.TYPE,
        Returns = "The binary data represented by this Base-64 text.", Examples = ["'aGk=' .from_base64 returns '6869'."],
        Others = ["Can be reverted via <@link tag BinaryTag.to_base64>BinaryTag.to_base64<@/link>."])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BinaryTag Tag_From_Base64(TextTag obj, TagData data)
    {
        string text = obj.Internal;
        try
        {
            byte[] bits = Convert.FromBase64String(text);
            if (bits == null)
            {
                data.Error("Invalid base64 input: '" + text + "'!");
                return null;
            }
            return new BinaryTag(bits);
        }
        catch (FormatException ex)
        {
            data.Error("Invalid base64 input: '" + text + "', with internal message: '" + ex.Message + "'!");
            return null;
        }
    }

#pragma warning restore 1591

    /// <summary>Converts the text tag to a string by returning the internal text.</summary>
    /// <returns>A string representation of this text tag.</returns>
    public override string ToString()
    {
        return Internal;
    }

    /// <summary>Adds a value to the object.</summary>
    /// <param name="first">The value on the left side of the operator.</param>
    /// <param name="val">The value to add.</param>
    [ObjectOperationAttribute(ObjectOperation.ADD, Input = TYPE)]
    public static TextTag Add(TextTag first, TextTag val)
    {
        return new TextTag(first.Internal + val.Internal);
    }
}
