using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Common;

namespace Frenetic.TagHandlers.Objects
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
            string[] baselist = list.Split('|');
            ListTag tlist = new ListTag();
            for (int i = 0; i < baselist.Length; i++)
            {
                tlist.ListEntries.Add(new TextTag(UnescapeTagBase.Unescape(baselist[i])));
            }
            return tlist;
        }
        
        /// <summary>
        /// Constructs a list tag from text input.
        /// </summary>
        /// <param name="list">The text input.</param>
        /// <returns>A valid list.</returns>
        public static ListTag For(TemplateObject list)
        {
            return list is ListTag ? (ListTag)list : For(list.ToString());
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override TemplateObject Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return this;
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name ListTag.size
                // @Group List Attributes
                // @ReturnType NumberTag
                // @Returns the number of entries in the list.
                // @Example "one|two|three" .size returns "3".
                // -->
                case "size":
                    return new NumberTag(ListEntries.Count).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.comma_separated
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list in a user-friendly comma-separated format.
                // @Example "one|two|three" .comma_separated returns "one, two, three".
                // -->
                case "comma_separated":
                    return new TextTag(ToCSString()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.space_separated
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list in a space-separated format.
                // @Example "one|two|three" .space_separated returns "one two three".
                // -->
                case "space_separated":
                    return new TextTag(ToSpaceString()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.unseparated
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list as an unseparated string.
                // @Example "one|two|three" .unseparated returns "onetwothree".
                // -->
                case "unseparated":
                    return new TextTag(ToFlatString()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.formatted
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the list in a user-friendly format.
                // @Example "one|two|three" .formatted returns "one, two, and three",
                // @Example "one|two" .formatted returns "one and two".
                // -->
                case "formatted":
                    return new TextTag(Formatted()).Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.reversed
                // @Group List Attributes
                // @ReturnType ListTag<Dynamic>
                // @Returns the list entirely backwards.
                // @Example "one|two|three" .reversed returns "three|two|one".
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
                // @ReturnType ListTag<Dynamic>
                // @Returns the list modified such that each entry is only included if the input modifier would return true for it.
                // @Example "one|two|three" .filter[true] returns "one|two|three".
                // @Example "1|2|3" .filter[<{var[value].equals[2]}>] returns "2".
                // -->
                case "filter":
                    {
                        ListTag newlist = new ListTag();
                        for (int i = 0; i < ListEntries.Count; i++)
                        {
                            Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>(data.Variables);
                            vars.Add("value", ListEntries[i]);
                            TemplateObject tobj = data.Modifiers[0].Parse(data.BaseColor, vars, data.mode, data.Error);
                            if ((tobj is BooleanTag ? (BooleanTag)tobj: BooleanTag.For(data, tobj.ToString())).Internal)
                            {
                                newlist.ListEntries.Add(ListEntries[i]);
                            }
                        }
                        return newlist.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.parse[<TextTag>]
                // @Group List Attributes
                // @ReturnType ListTag<Dynamic>
                // @Returns the list modified such that each entry is modified to be what the input modifier would return for it.
                // @Example "one|two|three" .parse[<{var[value].to_upper}>] returns "ONE|TWO|THREE".
                // -->
                case "parse":
                    {
                        ListTag newlist = new ListTag();
                        for (int i = 0; i < ListEntries.Count; i++)
                        {
                            Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>(data.Variables);
                            vars.Add("value", ListEntries[i]);
                            newlist.ListEntries.Add(data.Modifiers[0].Parse(data.BaseColor, vars, data.mode, data.Error));
                        }
                        return newlist.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ListTag.first
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns the first entry in the list.
                // @Example "one|two|three" .first returns "one".
                // -->
                case "first":
                    if (ListEntries.Count == 0)
                    {
                        data.Error("Read 'first' tag on empty list!");
                        return new TextTag("&{NULL}");
                    }
                    return ListEntries[0].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.random
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns a random entry from the list
                // @Example "one|two|three" .random returns "one", "two", or "three".
                // -->
                case "random":
                    if (ListEntries.Count == 0)
                    {
                        data.Error("Read 'random' tag on empty list!");
                        return new TextTag("&{NULL}");
                    }
                    return ListEntries[data.TagSystem.CommandSystem.random.Next(ListEntries.Count)].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.last
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns the last entry in the list.
                // @Example "one|two|three" .last returns "three".
                // -->
                case "last":
                    if (ListEntries.Count == 0)
                    {
                        data.Error("Read 'last' tag on empty list!");
                        return new TextTag("&{NULL}");
                    }
                    return ListEntries[ListEntries.Count - 1].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.get[<TextTag>]
                // @Group List Entries
                // @ReturnType Dynamic
                // @Returns the specified entry in the list.
                // @Other note that indices are one-based.
                // @Example "one|two|three" .get[2] returns "two".
                // -->
                case "get":
                    {
                        // TODO: Integer tag
                        string modif = data.GetModifier(0);
                        NumberTag num = NumberTag.For(data, modif);
                        if (ListEntries.Count == 0)
                        {
                            data.Error("Read 'get' tag on empty list!");
                            return new TextTag("&{NULL}");
                        }
                        if (num == null)
                        {
                            data.Error("Invalid number input: '" + modif + "'!");
                            return new TextTag("&{NULL}");
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
                // @Name ListTag.range[<TextTag>,<TextTag>]
                // @Group List Entries
                // @ReturnType ListTag<Dynamic>
                // @Returns the specified set of entries in the list.
                // @Other note that indices are one-based.
                // @Example "one|two|three|four" .range[2,3] returns "two|three".
                // @Example "one|two|three" .range[2,2] returns "two".
                // -->
                case "range":
                    {
                        string modif = data.GetModifier(0);
                        string[] split = modif.Split(',');
                        if (split.Length != 2)
                        {
                            data.Error("Invalid comma-separated-twin-number input: '" + modif + "'!");
                            return new TextTag("&{NULL}");
                        }
                        NumberTag num1 = NumberTag.For(data, split[0]);
                        NumberTag num2 = NumberTag.For(data, split[1]);
                        if (ListEntries.Count == 0)
                        {
                            data.Error("Read 'range' tag on empty list!");
                            return new TextTag("&{NULL}");
                        }
                        if (num1 == null || num2 == null)
                        {
                            data.Error("Invalid number input: '" + modif + "'!");
                            return new TextTag("&{NULL}");
                        }
                        int number = (int)num1.Internal - 1;
                        int number2 = (int)num1.Internal - 1;
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
                            // TODO: Queue level error!
                            number = ListEntries.Count - 1;
                        }
                        if (number2 >= ListEntries.Count)
                        {
                            // TODO: Queue level error!
                            number2 = ListEntries.Count - 1;
                        }
                        if (number2 < number)
                        {
                            // TODO: Queue level error!
                            return new ListTag().Handle(data.Shrink());
                        }
                        List<TemplateObject> Entries = new List<TemplateObject>();
                        for (int i = number; i <= number2; i++)
                        {
                            Entries.Add(ListEntries[i]);
                        }
                        return new ListTag(Entries).Handle(data.Shrink());
                    }
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
                sb.Append(EscapeTagBase.Escape(ListEntries[i].ToString()));
                if (i + 1 < ListEntries.Count)
                {
                    sb.Append("|");
                }
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
