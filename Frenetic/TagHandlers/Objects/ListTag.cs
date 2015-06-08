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
        /// <param name="entries">The entries</param>
        public ListTag(List<TemplateObject> entries)
        {
            ListEntries = new List<TemplateObject>(entries);
        }

        /// <summary>
        /// Constructs a list tag from a list of textual entries.
        /// </summary>
        /// <param name="entries">The textual entries</param>
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
        /// <param name="list">The text input</param>
        public ListTag(string list)
        {
            string[] baselist = list.Split('|');
            ListEntries = new List<TemplateObject>();
            for (int i = 0; i < baselist.Length; i++)
            {
                ListEntries.Add(new TextTag(UnescapeTags.Unescape(baselist[i])));
            }
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
                // @Name ListTag.size
                // @Group List Attributes
                // @ReturnType TextTag
                // @Returns the number of entries in the list.
                // @Example "one|two|three" .size returns "3".
                // -->
                case "size":
                    return new TextTag(ListEntries.Count.ToString()).Handle(data.Shrink());
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
                // @ReturnType ListTag
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
                // @Name ListTag.first
                // @Group List Attributes
                // @ReturnType Dynamic
                // @Returns the first entry in the list.
                // @Example "one|two|three" .first returns "one".
                // -->
                case "first":
                    if (ListEntries.Count == 0)
                    {
                        return new TextTag("&null").Handle(data.Shrink());
                    }
                    return ListEntries[0].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.random
                // @Group List Attributes
                // @ReturnType Dynamic
                // @Returns a random entry from the list
                // @Example "one|two|three" .random returns "one", "two", or "three".
                // -->
                case "random":
                    if (ListEntries.Count == 0)
                    {
                        return new TextTag("&null").Handle(data.Shrink());
                    }
                    return ListEntries[data.TagSystem.CommandSystem.random.Next(ListEntries.Count)].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.last
                // @Group List Attributes
                // @ReturnType Dynamic
                // @Returns the last entry in the list.
                // @Example "one|two|three" .last returns "three".
                // -->
                case "last":
                    if (ListEntries.Count == 0)
                    {
                        return new TextTag("&null").Handle(data.Shrink());
                    }
                    return ListEntries[ListEntries.Count - 1].Handle(data.Shrink());
                // <--[tag]
                // @Name ListTag.get[<TextTag>]
                // @Group List Attributes
                // @ReturnType Dynamic
                // @Returns the specified entry in the list.
                // @Other note that indices are one-based.
                // @Example "one|two|three" .get[2] returns "two".
                // -->
                case "get":
                    {
                        int number = FreneticUtilities.StringToInt(data.GetModifier(0)) - 1;
                        if (ListEntries.Count == 0)
                        {
                            return new TextTag("&null").Handle(data.Shrink());
                        }
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
                // @Group List Attributes
                // @ReturnType ListTag<Dynamic>
                // @Returns the specified set of entries in the list.
                // @Other note that indices are one-based.
                // @Example "one|two|three|four" .range[2,3] returns "two|three".
                // @Example "one|two|three" .range[2,1] returns an empty list.
                // @Example "one|two|three" .range[2,2] returns "two".
                // -->
                case "range":
                    {
                        string[] split = data.GetModifier(0).Split(',');
                        if (split.Length != 2)
                        {
                            return new TextTag("&null").Handle(data.Shrink());
                        }
                        if (ListEntries.Count == 0)
                        {
                            return new TextTag("&null").Handle(data.Shrink());
                        }
                        int number = FreneticUtilities.StringToInt(split[0]) - 1;
                        int number2 = FreneticUtilities.StringToInt(split[1]) - 1;
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
                            number = ListEntries.Count - 1;
                        }
                        if (number2 >= ListEntries.Count)
                        {
                            number2 = ListEntries.Count - 1;
                        }
                        if (number2 < number)
                        {
                            return new ListTag("").Handle(data.Shrink());
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
        /// <returns>A list of strings</returns>
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
        /// <returns>A string representation of the list</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ListEntries.Count; i++)
            {
                sb.Append(EscapeTags.Escape(ListEntries[i].ToString()));
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
