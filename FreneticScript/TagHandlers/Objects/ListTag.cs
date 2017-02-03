using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.CommandSystem;

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
        public List<TemplateObject> Internal;

        /// <summary>
        /// Constructs a new list tag.
        /// </summary>
        public ListTag()
        {
            Internal = new List<TemplateObject>();
        }

        /// <summary>
        /// Constructs a list tag from a list of entries.
        /// </summary>
        /// <param name="entries">The entries.</param>
        public ListTag(List<TemplateObject> entries)
        {
            Internal = new List<TemplateObject>(entries);
        }

        /// <summary>
        /// Constructs a list tag from a list of textual entries.
        /// </summary>
        /// <param name="entries">The textual entries.</param>
        public ListTag(List<string> entries)
        {
            Internal = new List<TemplateObject>();
            for (int i = 0; i < entries.Count; i++)
            {
                Internal.Add(new TextTag(entries[i]));
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
                tlist.Internal.Add(tab.InputValue);
            }
            return tlist;
        }

        /// <summary>
        /// Constructs a list tag from text input.
        /// </summary>
        /// <param name="data">The relevant tag data, if any.</param>
        /// <param name="input">The list input.</param>
        /// <returns>A valid list.</returns>
        public static ListTag For(TagData data, TemplateObject input)
        {
            return input as ListTag ?? (input is TextTag ? For(input.ToString()) : new ListTag(new List<TemplateObject>() { input }));
        }
        
        /// <summary>
        /// Constructs a list tag from text input.
        /// </summary>
        /// <param name="input">The list input.</param>
        /// <returns>A valid list.</returns>
        public static ListTag For(TemplateObject input)
        {
            return input as ListTag ?? (input is TextTag ? For(input.ToString()) : new ListTag(new List<TemplateObject>() { input }));
        }

        /// <summary>
        /// Creates a ListTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid list tag.</returns>
        public static TemplateObject CreateFor(TagData dat, TemplateObject input)
        {
            return input as ListTag ?? (input is TextTag ? For(input.ToString()) : new ListTag(new List<TemplateObject>() { input }));
        }
        /// <summary>
        /// The ListTag type.
        /// </summary>
        public const string TYPE = "listtag";

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "size", Group = "List Attributes", ReturnType = IntegerTag.TYPE, Returns = "The number of entries in the list.",
            Examples = new string[] { "'one|two|three|' .size returns '3'." })]
        public static TemplateObject Tag_Size(TagData data, TemplateObject obj)
        {
            return new NumberTag((obj as ListTag).Internal.Count);
        }

        [TagMeta(TagType = TYPE, Name = "comma_separated", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list in a user-friendly comma-separated format.",
            Examples = new string[] { "'one|two|three|' .comma_separated returns 'one, two, three'." })]
        public static TemplateObject Tag_Comma_Separated(TagData data, TemplateObject obj)
        {
            return new TextTag((obj as ListTag).ToCSString());
        }

        [TagMeta(TagType = TYPE, Name = "space_separated", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list in a space-separated format.",
            Examples = new string[] { "'one|two|three|' .space_separated returns 'one two three'." })]
        public static TemplateObject Tag_Space_Separated(TagData data, TemplateObject obj)
        {
            return new TextTag((obj as ListTag).ToSpaceString());
        }

        [TagMeta(TagType = TYPE, Name = "unseparated", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list as an unseparated string.",
            Examples = new string[] { "'one|two|three|' .unseparated returns 'onetwothree'." })]
        public static TemplateObject Tag_Unseparated(TagData data, TemplateObject obj)
        {
            return new TextTag((obj as ListTag).ToFlatString());
        }

        [TagMeta(TagType = TYPE, Name = "formatted", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list in a user-friendly format.",
            Examples = new string[] { "'one|two|three|' .formatted returns 'one, two, and three'." })]
        public static TemplateObject Tag_Formatted(TagData data, TemplateObject obj)
        {
            return new TextTag((obj as ListTag).Formatted());
        }

        [TagMeta(TagType = TYPE, Name = "reversed", Group = "List Attributes", ReturnType = ListTag.TYPE, Returns = "The list entirely backwards.",
            Examples = new string[] { "'one|two|three|' .reversed returns 'three|two|one|'." })]
        public static TemplateObject Tag_Reversed(TagData data, TemplateObject obj)
        {
            ListTag newlist = new ListTag((obj as ListTag).Internal);
            newlist.Internal.Reverse();
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "filter", Group = "List Attributes", ReturnType = ListTag.TYPE, 
            Returns = "The list modified such that each entry is only included if the input modifier would return true for it.",
            Examples = new string[] { "'one|two|three|' .filter[true] returns 'one|two|three|'.", "'1|2|3|' .filter[<{[filter_value].equals[2]}>] returns '2|'." })]
        public static TemplateObject Tag_Filter(TagData data, TemplateObject obj)
        {
            List<TemplateObject> Internal = (obj as ListTag).Internal;
            ListTag newlist = new ListTag();
            for (int i = 0; i < Internal.Count; i++)
            {
                // TODO: Restore: vars["filter_value"] = new ObjectHolder() { Internal = ListEntries[i] };
                TemplateObject tobj = data.InputKeys[data.cInd].Variable.Parse(data.BaseColor, data.mode, data.Error, data.CSE);
                if ((tobj is BooleanTag ? (BooleanTag)tobj : BooleanTag.For(data, tobj.ToString())).Internal)
                {
                    newlist.Internal.Add(Internal[i]);
                }
            }
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "parse", Group = "List Attributes", ReturnType = ListTag.TYPE, 
            Returns = "list modified such that each entry is modified to be what the input modifier would return for it.",
            Examples = new string[] { "'one|two|three|' .parse[<{[parse_value].to_upper}>] returns 'ONE|TWO|THREE|'." })]
        public static TemplateObject Tag_Parse(TagData data, TemplateObject obj)
        {
            List<TemplateObject> Internal = (obj as ListTag).Internal;
            ListTag newlist = new ListTag();
            for (int i = 0; i < Internal.Count; i++)
            {
                // TODO: Restore: vars["parse_value"] = new ObjectHolder() { Internal = ListEntries[i] };
                newlist.Internal.Add(data.InputKeys[data.cInd].Variable.Parse(data.BaseColor, data.mode, data.Error, data.CSE));
            }
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "first", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "The first entry in the list.",
            Examples = new string[] { "'one|two|three|' .first.as[TextTag] returns 'one'." })]
        public static TemplateObject Tag_First(TagData data, TemplateObject obj)
        {
            List<TemplateObject> Internal = (obj as ListTag).Internal;
            if (Internal.Count == 0)
            {
                data.Error("Read 'first' tag on empty list!");
                return new NullTag();
            }
            return new DynamicTag(Internal[0]);
        }

        [TagMeta(TagType = TYPE, Name = "random", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "A random entry from the list.",
            Examples = new string[] { "'one|two|three|' .random returns 'one', 'two', or 'three'." })]
        public static TemplateObject Tag_Random(TagData data, TemplateObject obj)
        {
            List<TemplateObject> Internal = (obj as ListTag).Internal;
            if (Internal.Count == 0)
            {
                data.Error("Read 'random' tag on empty list!");
                return new NullTag();
            }
            return new DynamicTag(Internal[data.TagSystem.CommandSystem.random.Next(Internal.Count)]);
        }

        [TagMeta(TagType = TYPE, Name = "last", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "A last entry in the list.",
            Examples = new string[] { "'one|two|three|' .last returns 'three'." })]
        public static TemplateObject Tag_Last(TagData data, TemplateObject obj)
        {
            List<TemplateObject> Internal = (obj as ListTag).Internal;
            if (Internal.Count == 0)
            {
                data.Error("Read 'last' tag on empty list!");
                return new NullTag();
            }
            return new DynamicTag(Internal[Internal.Count - 1]);
        }

        [TagMeta(TagType = TYPE, Name = "get", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "A specified entry in the list.",
            Examples = new string[] { "'one|two|three|' .get[2] returns 'two'." })]
        public static TemplateObject Tag_Get(TagData data, TemplateObject obj)
        {
            List<TemplateObject> Internal = (obj as ListTag).Internal;
            TemplateObject modif = data.GetModifierObject(0);
            IntegerTag num = IntegerTag.For(data, modif);
            if (Internal.Count == 0)
            {
                data.Error("Read 'get' tag on empty list!");
                return new NullTag();
            }
            int number = (int)num.Internal - 1;
            if (number < 0)
            {
                number = 0;
            }
            if (number >= Internal.Count)
            {
                number = Internal.Count - 1;
            }
            return new DynamicTag(Internal[number]);
        }

        [TagMeta(TagType = TYPE, Name = "range", Group = "List Attributes", ReturnType = ListTag.TYPE, Returns = "The specified set of entries in the list.",
            Examples = new string[] { "'one|two|three|four|' .range[2|3] returns 'two|three|'.", "'one|two|three|' .range[2|2] returns 'two|'." }, 
            Others = new String[] { "Note that indices are one-based." })]
        public static TemplateObject Tag_Range(TagData data, TemplateObject obj)
        {
            List<TemplateObject> Internal = (obj as ListTag).Internal;
            ListTag inputs = ListTag.For(data.GetModifierObject(0));
            if (inputs.Internal.Count < 2)
            {
                data.Error("Invalid substring tag! Not two entries in the list!");
                return new NullTag();
            }
            int number = (int)IntegerTag.For(data, inputs.Internal[0]).Internal - 1;
            int number2 = (int)IntegerTag.For(data, inputs.Internal[1]).Internal - 1;
            if (Internal.Count == 0)
            {
                data.Error("Read 'range' tag on empty list!");
                return new NullTag();
            }
            if (number < 0)
            {
                number = 0;
            }
            if (number2 < 0)
            {
                number2 = 0;
            }
            if (number >= Internal.Count)
            {
                data.Error("Invalid range tag!");
                return new NullTag();
            }
            if (number2 >= Internal.Count)
            {
                data.Error("Invalid range tag!");
                return new NullTag();
            }
            if (number2 < number)
            {
                return new ListTag().Handle(data.Shrink());
            }
            List<TemplateObject> Entries = new List<TemplateObject>();
            for (int i = number; i <= number2; i++)
            {
                Entries.Add(Internal[i]);
            }
            return new ListTag(Entries);
        }

        [TagMeta(TagType = TYPE, Name = "include", Group = "List Attributes", ReturnType = ListTag.TYPE, Returns = "A list with the input list added to the end.",
            Examples = new string[] { "'one|two|three|' .include[four|five|] returns 'one|two|three|four|five|'." })]
        public static TemplateObject Tag_Include(TagData data, TemplateObject obj)
        {
            ListTag newlist = new ListTag((obj as ListTag).Internal);
            newlist.Internal.AddRange(For(data.GetModifierObject(0)).Internal);
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "insert", Group = "List Attributes", ReturnType = ListTag.TYPE, 
            Returns = "A list with the input list added after an index specified as the first item in the list (index is not included in the final list).",
            Examples = new string[] { "'one|two|three|' .insert[1|a|b|] returns 'one|a|b|two|three|'." },
            Others = new String[] { "Note that indices are one-based.", "Specify 0 as the index to insert at the beginning." })]
        public static TemplateObject Tag_Insert(TagData data, TemplateObject obj)
        {
            ListTag modif = For(data.GetModifierObject(0));
            if (modif.Internal.Count == 0)
            {
                data.Error("Empty list to insert!");
                return new NullTag();
            }
            IntegerTag index = IntegerTag.For(data, modif.Internal[0]);
            modif.Internal.RemoveAt(0);
            ListTag newlist = new ListTag((obj as ListTag).Internal);
            if (index.Internal > newlist.Internal.Count)
            {
                index.Internal = newlist.Internal.Count;
            }
            newlist.Internal.InsertRange((int)index.Internal, modif.Internal);
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.",
            Examples = new string[] { "'1|2|3|' .duplicate returns '1|2|3|'." })]
        public static TemplateObject Tag_Duplicate(TagData data, TemplateObject obj)
        {
            return new ListTag((obj as ListTag).Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of the tag.",
            Examples = new string[] { "'true' .type returns 'listtag'." })]
        public static TemplateObject Tag_Type(TagData data, TemplateObject obj)
        {
            return new TagTypeTag(data.TagSystem.Type_List);
        }

#pragma warning restore 1591

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
                // TODO: Sort, styled like filter/parse tags
                
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
            List<string> list = new List<string>(Internal.Count);
            for (int i = 0; i < Internal.Count; i++)
            {
                list.Add(Internal[i].ToString());
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
            for (int i = 0; i < Internal.Count; i++)
            {
                sb.Append(EscapeTagBase.Escape(Internal[i].ToString())).Append("|");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the list as a comma-separated string (no escaping).
        /// </summary>
        public string ToCSString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Internal.Count; i++)
            {
                sb.Append(Internal[i].ToString());
                if (i + 1 < Internal.Count)
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
            for (int i = 0; i < Internal.Count; i++)
            {
                sb.Append(Internal[i].ToString());
                if (i + 1 < Internal.Count)
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
            for (int i = 0; i < Internal.Count; i++)
            {
                sb.Append(Internal[i].ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the list as a comma-separated string with 'and' and similar constructs.
        /// </summary>
        public string Formatted()
        {
            if (Internal.Count == 2)
            {
                return (Internal[0] + " and " + Internal[1]);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Internal.Count; i++)
            {
                sb.Append(Internal[i].ToString());
                if (i + 2 == Internal.Count)
                {
                    sb.Append(", and ");
                }
                else if (i + 1 < Internal.Count)
                {
                    sb.Append(", ");
                }
            }
            return sb.ToString();
        }
    }
}
