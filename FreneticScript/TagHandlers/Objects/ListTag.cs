using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a list as a usable tag.
    /// </summary>
    public class ListTag: TemplateObject
    {
        // <--[object]
        // @Type ListTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents a list of objects.
        // -->

        /// <summary>
        /// The list this ListTag represents.
        /// </summary>
        public List<TemplateObject> ListEntries;

        /// <summary>
        /// Constructs a new list tag.
        /// </summary>
        public ListTag()
        {
            ListEntries = new List<TemplateObject>();
        }

        /// <summary>
        /// Constructs a list tag from a list of entries.
        /// </summary>
        /// <param name="entries">The entries.</param>
        public ListTag(List<TemplateObject> entries)
        {
            ListEntries = new List<TemplateObject>(entries);
        }

        /// <summary>
        /// Constructs a list tag from a list of textual entries.
        /// </summary>
        /// <param name="entries">The textual entries.</param>
        public ListTag(List<string> entries)
        {
            ListEntries = new List<TemplateObject>();
            for (int i = 0; i < entries.Count; i++)
            {
                ListEntries.Add(new TextTag(entries[i]));
            }
        }

        /// <summary>
        /// Constructs a list tag from text input.
        /// </summary>
        /// <param name="list">The text input.</param>
        /// <returns>A valid list.</returns>
        public static ListTag For(string list)
        {
            string[] baselist = list.SplitFast('|');
            ListTag tlist = new ListTag();
            for (int i = 0; i < baselist.Length; i++)
            {
                if (i == baselist.Length - 1 && baselist[i].Length == 0)
                {
                    break;
                }
                string dat = UnescapeTagBase.Unescape(baselist[i]);
                TextArgumentBit tab = new TextArgumentBit(dat, false);
                tlist.ListEntries.Add(tab.InputValue);
            }
            return tlist;
        }
        
        /// <summary>
        /// Constructs a list tag from text input.
        /// </summary>
        /// <param name="list">The list input.</param>
        /// <returns>A valid list.</returns>
        public static ListTag For(TemplateObject list)
        {
            return list is ListTag ? (ListTag)list : (list is TextTag ? For(list.ToString()): new ListTag(new List<TemplateObject>() { list }));
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
                // @Name ListTag.size
                // @Group List Attributes
                // @ReturnType NumberTag
                // @Returns the number of entries in the list.
                // @Example "one|two|three|" .size returns "3".
                // -->
                case "size":
                    return new NumberTag(ListEntries.Count).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.comma_separated
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list in a user-friendly comma-separated format.
                // @Example "one|two|three|" .comma_separated returns "one, two, three".
                // -->
                case "comma_separated":
                    return new TextTag(ToCSString()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.space_separated
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list in a space-separated format.
                // @Example "one|two|three|" .space_separated returns "one two three".
                // -->
                case "space_separated":
                    return new TextTag(ToSpaceString()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.unseparated
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list as an unseparated string.
                // @Example "one|two|three|" .unseparated returns "onetwothree".
                // -->
                case "unseparated":
                    return new TextTag(ToFlatString()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.formatted
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list in a user-friendly format.
                // @Example "one|two|three|" .formatted returns "one, two, and three",
                // @Example "one|two|" .formatted returns "one and two".
                // -->
                case "formatted":
                    return new TextTag(Formatted()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.reversed
                // @Group List Attributes
                // @ReturnType ListTag
                // @Returns the list entirely backwards.
                // @Example "one|two|three|" .reversed returns "three|two|one|".
                // -->
                case "reversed":
                    {
                        ListTag newlist = new ListTag(ListEntries);
                        newlist.ListEntries.Reverse();
                        return newlist.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.filter[<BooleanTag>]
                // @Group List Attributes
                // @ReturnType ListTag
                // @Returns the list modified such that each entry is only included if the input modifier would return true for it.
                // @Example "one|two|three|" .filter[true] returns "one|two|three|".
                // @Example "1|2|3|" .filter[<{var[value].equals[2]}>] returns "2|".
                // -->
                case "filter":
                    {
                        ListTag newlist = new ListTag();
                        for (int i = 0; i < ListEntries.Count; i++)
                        {
                            Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>(data.Variables);
                            vars.Add("value", ListEntries[i]);
                            TemplateObject tobj = data.InputKeys[data.cInd].Variable.Parse(data.BaseColor, vars, data.mode, data.Error);
                            if ((tobj is BooleanTag ? (BooleanTag)tobj : BooleanTag.For(data, tobj.ToString())).Internal)
                            {
                                newlist.ListEntries.Add(ListEntries[i]);
                            }
                        }
                        return newlist.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.parse[<TextTag>]
                // @Group List Attributes
                // @ReturnType ListTag
                // @Returns the list modified such that each entry is modified to be what the input modifier would return for it.
                // @Example "one|two|three|" .parse[<{var[value].to_upper}>] returns "ONE|TWO|THREE|".
                // -->
                case "parse":
                    {
                        ListTag newlist = new ListTag();
                        for (int i = 0; i < ListEntries.Count; i++)
                        {
                            Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>(data.Variables);
                            vars.Add("value", ListEntries[i]);
                            newlist.ListEntries.Add(data.InputKeys[data.cInd].Variable.Parse(data.BaseColor, vars, data.mode, data.Error));
                        }
                        return newlist.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.first
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns the first entry in the list.
                // @Example "one|two|three|" .first returns "one".
                // -->
                case "first":
                    if (ListEntries.Count == 0)
                    {
                        data.Error("Read 'first' tag on empty list!");
                        return new NullTag();
                    }
                    return ListEntries[0].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.random
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns a random entry from the list
                // @Example "one|two|three|" .random returns "one", "two", or "three".
                // -->
                case "random":
                    if (ListEntries.Count == 0)
                    {
                        data.Error("Read 'random' tag on empty list!");
                        return new NullTag();
                    }
                    return ListEntries[data.TagSystem.CommandSystem.random.Next(ListEntries.Count)].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.last
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns the last entry in the list.
                // @Example "one|two|three|" .last returns "three".
                // -->
                case "last":
                    if (ListEntries.Count == 0)
                    {
                        data.Error("Read 'last' tag on empty list!");
                        return new NullTag();
                    }
                    return ListEntries[ListEntries.Count - 1].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.get[<TextTag>]
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns the specified entry in the list.
                // @Other note that indices are one-based.
                // @Example "one|two|three|" .get[2] returns "two".
                // -->
                case "get":
                    {
                        TemplateObject modif = data.GetModifierObject(0);
                        IntegerTag num = IntegerTag.For(data, modif);
                        if (ListEntries.Count == 0)
                        {
                            data.Error("Read 'get' tag on empty list!");
                            return new NullTag();
                        }
                        if (num == null)
                        {
                            data.Error("Invalid integer input: '" + TagParser.Escape(modif.ToString()) + "'!");
                            return new NullTag();
                        }
                        int number = (int)num.Internal - 1;
                        if (number < 0)
                        {
                            number = 0;
                        }
                        if (number >= ListEntries.Count)
                        {
                            number = ListEntries.Count - 1;
                        }
                        return ListEntries[number].Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.range[<IntegerTag>,<IntegerTag>]
                // @Group List Entries
                // @ReturnType ListTag
                // @Returns the specified set of entries in the list.
                // @Other note that indices are one-based.
                // @Example "one|two|three|four|" .range[2,3] returns "two|three|".
                // @Example "one|two|three|" .range[2,2] returns "two|".
                // -->
                case "range":
                    {
                        string modif = data.GetModifier(0);
                        string[] split = modif.SplitFast(',');
                        if (split.Length != 2)
                        {
                            data.Error("Invalid comma-separated-twin-number input: '" + TagParser.Escape(modif) + "'!");
                            return new NullTag();
                        }
                        IntegerTag num1 = IntegerTag.For(data, split[0]);
                        IntegerTag num2 = IntegerTag.For(data, split[1]);
                        if (ListEntries.Count == 0)
                        {
                            data.Error("Read 'range' tag on empty list!");
                            return new NullTag();
                        }
                        if (num1 == null || num2 == null)
                        {
                            data.Error("Invalid integer input: '" + TagParser.Escape(modif) + "'!");
                            return new NullTag();
                        }
                        int number = (int)num1.Internal - 1;
                        int number2 = (int)num2.Internal - 1;
                        if (number < 0)
                        {
                            number = 0;
                        }
                        if (number2 < 0)
                        {
                            number2 = 0;
                        }
                        if (number >= ListEntries.Count)
                        {
                            data.Error("Invalid range tag!");
                            return new NullTag();
                        }
                        if (number2 >= ListEntries.Count)
                        {
                            data.Error("Invalid range tag!");
                            return new NullTag();
                        }
                        if (number2 < number)
                        {
                            data.Error("Invalid range tag!");
                            return new NullTag();
                        }
                        List<TemplateObject> Entries = new List<TemplateObject>();
                        for (int i = number; i <= number2; i++)
                        {
                            Entries.Add(ListEntries[i]);
                        }
                        return new ListTag(Entries).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.include[<ListTag>]
                // @Group List Entries
                // @ReturnType ListTag
                // @Returns a list with the input list added to the end.
                // @Other note that indices are one-based.
                // @Example "one|two|three|" .include[four|five] returns "one|two|three|four|five|".
                // -->
                case "include":
                    {
                        ListTag newlist = new ListTag(ListEntries);
                        newlist.ListEntries.AddRange(For(data.GetModifierObject(0)).ListEntries);
                        return newlist.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.insert[<IntegerTag>|<ListTag>]
                // @Group List Entries
                // @ReturnType ListTag
                // @Returns a list with the input list added after an index specified as the first item in the list (index is not included in the final list).
                // @Other note that indices are one-based.
                // @Other specify 0 as the index to insert at the beginning.
                // @Example "one|two|three|" .insert[1|a|b|] returns "one|a|b|two|three|".
                // -->
                case "insert":
                    {
                        ListTag modif = For(data.GetModifierObject(0));
                        if (modif.ListEntries.Count == 0)
                        {
                            data.Error("Empty list to insert!");
                            return new NullTag();
                        }
                        IntegerTag index = IntegerTag.For(data, modif.ListEntries[0]);
                        modif.ListEntries.RemoveAt(0);
                        ListTag newlist = new ListTag(ListEntries);
                        if (index.Internal > newlist.ListEntries.Count)
                        {
                            index.Internal = newlist.ListEntries.Count;
                        }
                        newlist.ListEntries.InsertRange((int)index.Internal, modif.ListEntries);
                        return newlist.Handle(data.Shrink());
                    }
                // Documented in TextTag.
                case "duplicate":
                    return new ListTag(ListEntries).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        /// <summary>
        /// Converts the ListTag to a list of strings.
        /// </summary>
        /// <returns>A list of strings.</returns>
        public List<string> ToStringList()
        {
            List<string> list = new List<string>(ListEntries.Count);
            for (int i = 0; i < ListEntries.Count; i++)
            {
                list.Add(ListEntries[i].ToString());
            }
            return list;
        }

        /// <summary>
        /// Converts the list tag to a string.
        /// </summary>
        /// <returns>A string representation of the list.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ListEntries.Count; i++)
            {
                sb.Append(EscapeTagBase.Escape(ListEntries[i].ToString())).Append("|");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the list as a comma-separated string (no escaping).
        /// </summary>
        public string ToCSString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ListEntries.Count; i++)
            {
                sb.Append(ListEntries[i].ToString());
                if (i + 1 < ListEntries.Count)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the list as a space-separated string (no escaping).
        /// </summary>
        public string ToSpaceString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ListEntries.Count; i++)
            {
                sb.Append(ListEntries[i].ToString());
                if (i + 1 < ListEntries.Count)
                {
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the list as an unseparated string (no escaping).
        /// </summary>
        public string ToFlatString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ListEntries.Count; i++)
            {
                sb.Append(ListEntries[i].ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the list as a comma-separated string with 'and' and similar constructs.
        /// </summary>
        public string Formatted()
        {
            if (ListEntries.Count == 2)
            {
                return (ListEntries[0] + " and " + ListEntries[1]);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ListEntries.Count; i++)
            {
                sb.Append(ListEntries[i].ToString());
                if (i + 2 == ListEntries.Count)
                {
                    sb.Append(", and ");
                }
                else if (i + 1 < ListEntries.Count)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
    }
}
