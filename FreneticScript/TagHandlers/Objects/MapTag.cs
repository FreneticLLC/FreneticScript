//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.CommandSystem;
using System.Runtime.CompilerServices;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a relationship between textual names and object data.
    /// </summary>
    [ObjectMeta(Name = MapTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Structural", Description = "Represents a relationship between textual names and object data.")]
    public class MapTag : TemplateObject, ListTagForm
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
            return tagTypeSet.Type_Map;
        }

        /// <summary>
        /// The <see cref="ListTag"/> value of this <see cref="ListTag"/>-like object.
        /// </summary>
        public ListTag ListForm
        {
            get
            {
                ListTag list = new ListTag(Internal.Count);
                foreach (KeyValuePair<string, TemplateObject> pair in Internal)
                {
                    list.Internal.Add(new TextTag(pair.Key + ":" + pair.Value));
                }
                return list;
            }
        }

        /// <summary>
        /// The internal dictionary that this MapTag represents.
        /// </summary>
        public Dictionary<string, TemplateObject> Internal;
        
        /// <summary>
        /// Constructs a MapTag from existing data.
        /// NOTE: This expects all keys to be lowercase!
        /// </summary>
        /// <param name="toUse">The data to use.</param>
        public MapTag(Dictionary<string, TemplateObject> toUse)
        {
            Internal = new Dictionary<string, TemplateObject>(toUse);
        }
        
        /// <summary>
        /// Constructs a MapTag without existing data.
        /// </summary>
        public MapTag()
        {
            Internal = new Dictionary<string, TemplateObject>();
        }

        /// <summary>
        /// Constructs a MapTag without existing data.
        /// </summary>
        /// <param name="capacity">The number of expected entries.</param>
        public MapTag(int capacity)
        {
            Internal = new Dictionary<string, TemplateObject>(capacity);
        }

        /// <summary>
        /// Helper validator to validate an argument as a map tag.
        /// </summary>
        /// <param name="validator">The validation helper.</param>
        public static void Validator(ArgumentValidation validator)
        {
            validator.ObjectValue = For(validator.ObjectValue);
        }

        /// <summary>
        /// Converts text to a map tag.
        /// Never null. Will ignore invalid entries.
        /// </summary>
        /// <param name="input">The input text.</param>
        /// <returns>The map represented by the input text.</returns>
        public static MapTag For(string input)
        {
            string[] dat = input.SplitFast('|');
            MapTag map = new MapTag(dat.Length);
            for (int i = 0; i < dat.Length; i++)
            {
                string[] kvp = dat[i].SplitFast(':');
                if (kvp.Length != 2)
                {
                    // TODO: Error?
                    continue;
                }
                string key = UnescapeTagBase.Unescape(kvp[0]).ToLowerFast();
                map.Internal[key] = new TextArgumentBit(UnescapeTagBase.Unescape(kvp[1]), false, true).InputValue;
            }
            return map;
        }

        /// <summary>
        /// Converts saved text to a map tag.
        /// Never null. Will ignore invalid entries.
        /// </summary>
        /// <param name="input">The input saved text.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>The map represented by the input text.</returns>
        public static MapTag CreateFromSaved(string input, TagData data)
        {
            string[] dat = input.SplitFast('|');
            MapTag map = new MapTag();
            for (int i = 0; i < dat.Length; i++)
            {
                string[] kvp = dat[i].SplitFast(':');
                if (kvp.Length != 2)
                {
                    // TODO: Error?
                    continue;
                }
                map.Internal[UnescapeTagBase.Unescape(kvp[0]).ToLowerFast()] = data.TagSystem.ParseFromSaved(UnescapeTagBase.Unescape(kvp[1]), data);
            }
            return map;
        }

        /// <summary>
        /// Converts a generic object to a map tag.
        /// Never null. Will ignore invalid entries.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <returns>The map represented by the input object.</returns>
        public static MapTag For(TemplateObject input)
        {
            switch (input)
            {
                case MapTag itag:
                    return itag;
                case DynamicTag dtag:
                    return For(dtag.Internal);
                default:
                    return For(input.ToString());
            }
        }

        /// <summary>
        /// Creates a MapTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid map tag.</returns>
        public static MapTag CreateFor(TemplateObject input, TagData dat)
        {
            return For(input);
        }

        /// <summary>
        /// The MapTag type.
        /// </summary>
        public const string TYPE = "map";

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MapTag Tag_Duplicate(MapTag obj, TagData data)
        {
            return new MapTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (MapTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(MapTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Types.Type_Map);
        }

        [TagMeta(TagType = TYPE, Name = "size", Group = "Map Entries", ReturnType = IntegerTag.TYPE, Returns = "The number of entries in the map.",
            Examples = new string[] { "'one:a|two:b' .size returns '2'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerTag Tag_Size(MapTag obj, TagData data)
        {
            return new IntegerTag(obj.Internal.Count);
        }

        [TagMeta(TagType = TYPE, Name = "keys", Group = "Map Entries", ReturnType = ListTag.TYPE, Returns = "A list of all keys in the map.",
            Examples = new string[] { "'one:a|two:b' .keys returns 'one|two|'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Keys(MapTag obj, TagData data)
        {
            return new ListTag(obj.Internal.Keys.ToList());
        }

        [TagMeta(TagType = TYPE, Name = "values", Group = "Map Entries", ReturnType = ListTag.TYPE, Returns = "A list of all values in the map.",
            Examples = new string[] { "'one:a|two:b' .values returns 'a|b|'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListTag Tag_Values(MapTag obj, TagData data)
        {
            ListTag list = new ListTag();
            list.Internal.AddRange(obj.Internal.Values);
            return list;
        }

        [TagMeta(TagType = TYPE, Name = "contains", Group = "Map Entries", ReturnType = BooleanTag.TYPE, Modifier = TextTag.TYPE,
            Returns = "Whether the map contains an entry with the specified key.", 
            Examples = new string[] { "'one:a|two:b' .contains[one] returns 'true'.", "'one:a|two:b' .contains[three] returns 'false'."})]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BooleanTag Tag_Contains(MapTag obj, TextTag modifier)
        {
            return BooleanTag.ForBool(obj.Internal.ContainsKey(modifier.Internal.ToLowerFast()));
        }

        [TagMeta(TagType = TYPE, Name = "get", Group = "Map Entries", ReturnType = DynamicTag.TYPE,
            Returns = "The specified entry value in the map.",
            Examples = new string[] { "'one:a|two:b' .get[one] returns 'a'.", "'one:a|two:b' .get[two] returns 'b'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_Get(MapTag obj, TagData data)
        {
            string modif = data.GetModifierCurrent().ToLowerFast();
            if (obj.Internal.TryGetValue(modif, out TemplateObject outp))
            {
                return new DynamicTag(outp);
            }
            data.Error("Unknown map entry: '" + modif + "'!");
            return null;
        }

#pragma warning restore 1591

        /// <summary>
        /// Gets a savable string representation of this map.
        /// </summary>
        /// <returns>The typed string representation.</returns>
        public override string GetSavableString()
        {
            StringBuilder toret = new StringBuilder(Internal.Count * 50 + 10);
            toret.Append(TYPE);
            toret.Append(SAVE_MARK);
            foreach (KeyValuePair<string, TemplateObject> entry in Internal)
            {
                toret.Append(EscapeTagBase.Escape(entry.Key)).Append(":").Append(EscapeTagBase.Escape(entry.Value.GetSavableString())).Append("|");
            }
            return toret.ToString().Substring(0, toret.Length - 1);
        }

        /// <summary>
        /// Gets a string representation of this map.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            if (Internal.Count == 0)
            {
                return "";
            }
            StringBuilder toret = new StringBuilder(Internal.Count * 100);
            foreach (KeyValuePair<string, TemplateObject> entry in Internal)
            {
                toret.Append(EscapeTagBase.Escape(entry.Key)).Append(":").Append(EscapeTagBase.Escape(entry.Value.ToString())).Append("|");
            }
            return toret.ToString().Substring(0, toret.Length - 1);
        }

        /// <summary>
        /// Gets a "clean" text form of an object for simpler output to debug logs, may have added colors or other details.
        /// </summary>
        /// <returns>The debug-friendly string.</returns>
        public override string GetDebugString()
        {
            if (Internal.Count == 0)
            {
                return "";
            }
            StringBuilder toret = new StringBuilder(Internal.Count * 100);
            foreach (KeyValuePair<string, TemplateObject> entry in Internal)
            {
                toret.Append(TextStyle.Separate).Append(entry.Key)
                    .Append(TextStyle.Minor).Append(": ").Append(TextStyle.Separate)
                    .Append(entry.Value.GetDebugString()).Append(TextStyle.Minor).Append(" | ");
            }
            return toret.ToString().Substring(0, toret.Length - 3);
        }

        /// <summary>
        /// Gets the sub-settable object, or null if none.
        /// </summary>
        /// <param name="tag">The map tag.</param>
        /// <param name="name">The sub-settable object name.</param>
        /// <returns>The sub-settable object, or null.</returns>
        [ObjectOperationAttribute(ObjectOperation.GETSUBSETTABLE, Input = TYPE)]
        public static TemplateObject GetSubSettable(MapTag tag, string name)
        {
            return tag.Internal.TryGetValue(name.ToLowerFast(), out TemplateObject value) ? value : null;
        }

        /// <summary>
        /// Sets a sub-object by name.
        /// </summary>
        /// <param name="tag">The map tag.</param>
        /// <param name="value">The value to insert.</param>
        /// <param name="name">The sub-object name to insert.</param>
        [ObjectOperationAttribute(ObjectOperation.SET, Input = TYPE)]
        public static void SetSubObject(MapTag tag, TemplateObject value, string name)
        {
            tag.Internal[name.ToLowerFast()] = value;
        }
    }
}
