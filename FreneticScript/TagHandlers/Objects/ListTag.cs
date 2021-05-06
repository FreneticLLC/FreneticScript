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
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a list as a usable tag.
    /// </summary>
    [ObjectMeta(Name = ListTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Structural", Description = "Represents a list of objects.")]
    public class ListTag: TemplateObject, IListTagForm
    {

        /// <summary>
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public override string GetTagTypeName()
        {
            return TYPE;
        }

        /// <summary>
        /// Return the type of this tag.
        /// </summary>
        /// <returns>The tag type.</returns>
        public override TagType GetTagType(TagTypes tagTypeSet)
        {
            return tagTypeSet.Type_List;
        }

        /// <summary>
        /// The <see cref="ListTag"/> value of this <see cref="ListTag"/>-like object.
        /// </summary>
        public ListTag ListForm
        {
            get
            {
                return this;
            }
        }

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
        /// Helper validator to validate an argument as a list tag.
        /// </summary>
        /// <param name="validator">The validation helper.</param>
        public static void Validator(ArgumentValidation validator)
        {
            validator.ObjectValue = CreateFor(validator.ObjectValue);
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
        /// Constructs a list tag from a list of entries.
        /// </summary>
        /// <param name="capacity">The number of expected entries.</param>
        public ListTag(int capacity)
        {
            Internal = new List<TemplateObject>(capacity);
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
                TextArgumentBit tab = new TextArgumentBit(dat, false, true, null);
                tlist.Internal.Add(tab.InputValue);
            }
            return tlist;
        }

        /// <summary>
        /// Constructs a list tag from saved text input.
        /// </summary>
        /// <param name="list">The saved text input.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>A valid list.</returns>
        public static ListTag CreateFromSaved(string list, TagData data)
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
                tlist.Internal.Add(data.TagSystem.ParseFromSaved(dat, data));
            }
            return tlist;
        }
        
        /// <summary>
        /// Creates a ListTag for the given input data.
        /// </summary>
        /// <param name="input">The text input.</param>
        /// <returns>A valid list tag.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag CreateFor(TemplateObject input)
        {
            return input switch
            {
                ListTag ltag => ltag,
                IListTagForm lform => lform.ListForm,
                DynamicTag dtag => CreateFor(dtag.Internal),
                TextTag _ => For(input.ToString()),
                _ => new ListTag(new List<TemplateObject>() { input }),
            };
        }

        /// <summary>
        /// Creates a ListTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid list tag.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag CreateFor(TemplateObject input, TagData dat)
        {
            return CreateFor(input);
        }

        /// <summary>
        /// The ListTag type.
        /// </summary>
        public const string TYPE = "list";

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "size", Group = "List Attributes", ReturnType = IntegerTag.TYPE, Returns = "The number of entries in the list.",
            Examples = new string[] { "'one|two|three|' .size returns '3'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Size(ListTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Count);
        }

        [TagMeta(TagType = TYPE, Name = "comma_separated", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list in a user-friendly comma-separated format.",
            Examples = new string[] { "'one|two|three|' .comma_separated returns 'one, two, three'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Comma_Separated(ListTag obj, TagData data)
        {
            return new TextTag(obj.ToCSString());
        }

        [TagMeta(TagType = TYPE, Name = "space_separated", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list in a space-separated format.",
            Examples = new string[] { "'one|two|three|' .space_separated returns 'one two three'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Space_Separated(ListTag obj, TagData data)
        {
            return new TextTag(obj.ToSpaceString());
        }

        [TagMeta(TagType = TYPE, Name = "unseparated", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list as an unseparated string.",
            Examples = new string[] { "'one|two|three|' .unseparated returns 'onetwothree'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Unseparated(ListTag obj, TagData data)
        {
            return new TextTag(obj.ToFlatString());
        }

        [TagMeta(TagType = TYPE, Name = "formatted", Group = "List Attributes", ReturnType = TextTag.TYPE, Returns = "The list in a user-friendly format.",
            Examples = new string[] { "'one|two|three|' .formatted returns 'one, two, and three'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextTag Tag_Formatted(ListTag obj, TagData data)
        {
            return new TextTag(obj.Formatted());
        }

        [TagMeta(TagType = TYPE, Name = "reversed", Group = "List Attributes", ReturnType = ListTag.TYPE, Returns = "The list entirely backwards.",
            Examples = new string[] { "'one|two|three|' .reversed returns 'three|two|one|'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Reversed(ListTag obj, TagData data)
        {
            ListTag newlist = new ListTag(obj.Internal);
            newlist.Internal.Reverse();
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "filter", Group = "List Attributes", ReturnType = ListTag.TYPE, 
            Returns = "The list modified such that each entry is only included if the input modifier would return true for it.",
            Examples = new string[] { "'one|two|three|' .filter[true] returns 'one|two|three|'.", "'1|2|3|' .filter[<{[filter_value].equals[2]}>] returns '2|'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Filter(ListTag obj, TagData data)
        {
            List<TemplateObject> Internal = obj.Internal;
            ListTag newlist = new ListTag();
            for (int i = 0; i < Internal.Count; i++)
            {
                // TODO: Restore: vars["filter_value"] = new ObjectHolder() { Internal = ListEntries[i] };
                TemplateObject tobj = data.Variables[data.cInd].Parse(data.ErrorAction, data.Runnable);
                if (BooleanTag.CreateFor(tobj, data).Internal)
                {
                    newlist.Internal.Add(Internal[i]);
                }
            }
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "parse", Group = "List Attributes", ReturnType = ListTag.TYPE, 
            Returns = "list modified such that each entry is modified to be what the input modifier would return for it.",
            Examples = new string[] { "'one|two|three|' .parse[<{[parse_value].to_upper}>] returns 'ONE|TWO|THREE|'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Parse(ListTag obj, TagData data)
        {
            List<TemplateObject> Internal = obj.Internal;
            ListTag newlist = new ListTag();
            for (int i = 0; i < Internal.Count; i++)
            {
                // TODO: Restore: vars["parse_value"] = new ObjectHolder() { Internal = ListEntries[i] };
                newlist.Internal.Add(data.Variables[data.cInd].Parse(data.ErrorAction, data.Runnable));
            }
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "first", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "The first entry in the list.",
            Examples = new string[] { "'one|two|three|' .first.as[TextTag] returns 'one'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_First(ListTag obj, TagData data)
        {
            List<TemplateObject> Internal = obj.Internal;
            if (Internal.Count == 0)
            {
                data.Error("Read 'first' tag on empty list!");
                return null;
            }
            return new DynamicTag(Internal[0]);
        }

        [TagMeta(TagType = TYPE, Name = "random", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "A random entry from the list.",
            Examples = new string[] { "'one|two|three|' .random returns 'one', 'two', or 'three'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_Random(ListTag obj, TagData data)
        {
            List<TemplateObject> Internal = obj.Internal;
            if (Internal.Count == 0)
            {
                data.Error("Read 'random' tag on empty list!");
                return null;
            }
            return new DynamicTag(Internal[data.TagSystem.Engine.random.Next(Internal.Count)]);
        }

        [TagMeta(TagType = TYPE, Name = "last", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "A last entry in the list.",
            Examples = new string[] { "'one|two|three|' .last returns 'three'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_Last(ListTag obj, TagData data)
        {
            List<TemplateObject> Internal = obj.Internal;
            if (Internal.Count == 0)
            {
                data.Error("Read 'last' tag on empty list!");
                return null;
            }
            return new DynamicTag(Internal[Internal.Count - 1]);
        }

        [TagMeta(TagType = TYPE, Name = "get", Group = "List Attributes", ReturnType = DynamicTag.TYPE, Returns = "A specified entry in the list.",
            Examples = new string[] { "'one|two|three|' .get[2] returns 'two'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_Get(ListTag obj, TagData data)
        {
            List<TemplateObject> Internal = obj.Internal;
            TemplateObject modif = data.GetModifierObjectCurrent();
            IntegerTag num = IntegerTag.For(modif, data);
            if (Internal.Count == 0)
            {
                data.Error("Read 'get' tag on empty list!");
                return null;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Range(ListTag obj, TagData data)
        {
            List<TemplateObject> Internal = obj.Internal;
            ListTag inputs = ListTag.CreateFor(data.GetModifierObjectCurrent());
            if (inputs.Internal.Count < 2)
            {
                data.Error("Invalid substring tag! Not two entries in the list!");
                return null;
            }
            int number = (int)IntegerTag.For(inputs.Internal[0], data).Internal - 1;
            int number2 = (int)IntegerTag.For(inputs.Internal[1], data).Internal - 1;
            if (Internal.Count == 0)
            {
                data.Error("Read 'range' tag on empty list!");
                return null;
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
                return null;
            }
            if (number2 >= Internal.Count)
            {
                data.Error("Invalid range tag!");
                return null;
            }
            if (number2 < number)
            {
                return new ListTag();
            }
            List<TemplateObject> Entries = new List<TemplateObject>();
            for (int i = number; i <= number2; i++)
            {
                Entries.Add(Internal[i]);
            }
            return new ListTag(Entries);
        }

        [TagMeta(TagType = TYPE, Name = "include", Modifier = TYPE, Group = "List Attributes", ReturnType = ListTag.TYPE, Returns = "A list with the input list added to the end.",
            Examples = new string[] { "'one|two|three|' .include[four|five|] returns 'one|two|three|four|five|'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Include(ListTag obj, ListTag modifier)
        {
            ListTag newlist = new ListTag(obj.Internal);
            newlist.Internal.AddRange(modifier.Internal);
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "insert", Group = "List Attributes", ReturnType = ListTag.TYPE, 
            Returns = "A list with the input list added after an index specified as the first item in the list (index is not included in the final list).",
            Examples = new string[] { "'one|two|three|' .insert[1|a|b|] returns 'one|a|b|two|three|'." },
            Others = new String[] { "Note that indices are one-based.", "Specify 0 as the index to insert at the beginning." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Insert(ListTag obj, TagData data)
        {
            ListTag modif = CreateFor(data.GetModifierObjectCurrent());
            if (modif.Internal.Count == 0)
            {
                data.Error("Empty list to insert (need to specify at least the insertion index value)!");
                return null;
            }
            IntegerTag index = IntegerTag.For(modif.Internal[0], data);
            modif.Internal.RemoveAt(0);
            ListTag newlist = new ListTag(obj.Internal);
            if (index.Internal > newlist.Internal.Count)
            {
                index.Internal = newlist.Internal.Count;
            }
            newlist.Internal.InsertRange((int)index.Internal, modif.Internal);
            return newlist;
        }

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Duplicate(ListTag obj, TagData data)
        {
            return new ListTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (ListTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(ListTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Types.Type_List);
        }

        // TODO: Sort, styled like filter/parse tags

#pragma warning restore 1591
            
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
        /// Converts the list tag to a savable string.
        /// </summary>
        /// <returns>A typed string representation of the list.</returns>
        public override string GetSavableString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(TYPE);
            sb.Append(SAVE_MARK);
            for (int i = 0; i < Internal.Count; i++)
            {
                sb.Append(EscapeTagBase.Escape(Internal[i].GetSavableString())).Append("|");
            }
            return sb.ToString();
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
        /// Gets a "clean" text form of an object for simpler output to debug logs, may have added colors or other details.
        /// </summary>
        /// <returns>The debug-friendly string.</returns>
        public override string GetDebugString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Internal.Count; i++)
            {
                sb.Append(TextStyle.Separate).Append(Internal[i].GetDebugString()).Append(TextStyle.Minor).Append(" | ");
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
