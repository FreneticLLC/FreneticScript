using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.CommandSystem;
using System.Runtime.CompilerServices;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a relationship between textual names and object data.
    /// </summary>
    public class MapTag : TemplateObject
    {
        // <--[object]
        // @Type MapTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents a relationship between textual names and object data.
        // -->

        /// <summary>
        /// The internal dictionary that this MapTag represents.
        /// </summary>
        public Dictionary<string, TemplateObject> Internal;

        /// <summary>
        /// Constructs a MapTag from existing data.
        /// NOTE: This expects all keys to be lowercase!
        /// </summary>
        /// <param name="toUse">The data to use.</param>
        public MapTag(Dictionary<string, ObjectHolder> toUse)
        {
            Dictionary<string, TemplateObject> temp = new Dictionary<string, TemplateObject>();
            foreach (KeyValuePair<string, ObjectHolder> obj in toUse)
            {
                temp[obj.Key] = obj.Value.Internal;
            }
            Internal = temp;
        }

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
        /// Converts text to a map tag.
        /// Never null. Will ignore invalid entries.
        /// </summary>
        /// <param name="input">The input text.</param>
        /// <returns>The map represented by the input text.</returns>
        public static MapTag For(string input)
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
                map.Internal[kvp[0].ToLowerFast()] = new TextArgumentBit(kvp[1], false).InputValue;
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
            return input as MapTag ?? For(input.ToString());
        }

        /// <summary>
        /// Converts a generic object to a map tag.
        /// Never null. Will ignore invalid entries.
        /// </summary>
        /// <param name="dat">The relevant tag data, if any.</param>
        /// <param name="input">The input object.</param>
        /// <returns>The map represented by the input object.</returns>
        public static MapTag For(TemplateObject input, TagData dat)
        {
            return input as MapTag ?? For(input.ToString());
        }

        /// <summary>
        /// Creates a MapTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid map tag.</returns>
        public static MapTag CreateFor(TemplateObject input, TagData dat)
        {
            switch (input)
            {
                case MapTag itag:
                    return itag;
                case DynamicTag dtag:
                    return CreateFor(dtag.Internal, dat);
                default:
                    return For(input.ToString());
            }
        }

        /// <summary>
        /// The MapTag type.
        /// </summary>
        public const string TYPE = "maptag";

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
            return new TagTypeTag(data.TagSystem.Type_Map);
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
            return new BooleanTag(obj.Internal.ContainsKey(modifier.Internal.ToLowerFast()));
        }

        [TagMeta(TagType = TYPE, Name = "get", Group = "Map Entries", ReturnType = DynamicTag.TYPE,
            Returns = "The specified entry value in the map.",
            Examples = new string[] { "'one:a|two:b' .get[one] returns 'a'.", "'one:a|two:b' .get[two] returns 'b'." })]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TemplateObject Tag_Get(MapTag obj, TagData data)
        {
            string modif = data.GetModifier(0).ToLowerFast();
            if (obj.Internal.TryGetValue(modif, out TemplateObject outp))
            {
                return new DynamicTag(outp);
            }
            data.Error("Unknown map entry: '" + TagParser.Escape(modif) + "'!");
            return new NullTag();
        }

#pragma warning restore 1591

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
        /// Sets a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to set it to.</param>
        public override void Set(string[] names, TemplateObject val)
        {
            if (names != null && names.Length == 1)
            {
                Internal[names[0].ToLowerFast()] = val;
                return;
            }
            if (names != null && names.Length > 1 && Internal.TryGetValue(names[0].ToLowerFast(), out TemplateObject obj))
            {
                string[] n2 = new string[names.Length - 1];
                Array.Copy(names, 1, n2, 0, n2.Length);
                obj.Set(n2, val);
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
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out TemplateObject obj))
            {
                string[] n2 = new string[names.Length - 1];
                Array.Copy(names, 1, n2, 0, n2.Length);
                obj.Add(n2, val);
                return;
            }
            base.Add(names, val);
        }

        /// <summary>
        /// Subtracts a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to subtract.</param>
        public override void Subtract(string[] names, TemplateObject val)
        {
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out TemplateObject obj))
            {
                string[] n2 = new string[names.Length - 1];
                Array.Copy(names, 1, n2, 0, n2.Length);
                obj.Subtract(n2, val);
                return;
            }
            base.Subtract(names, val);
        }

        /// <summary>
        /// Multiplies a value by a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to multiply.</param>
        public override void Multiply(string[] names, TemplateObject val)
        {
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out TemplateObject obj))
            {
                string[] n2 = new string[names.Length - 1];
                Array.Copy(names, 1, n2, 0, n2.Length);
                obj.Multiply(n2, val);
                return;
            }
            base.Multiply(names, val);
        }

        /// <summary>
        /// Divides a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to divide.</param>
        public override void Divide(string[] names, TemplateObject val)
        {
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out TemplateObject obj))
            {
                string[] n2 = new string[names.Length - 1];
                Array.Copy(names, 1, n2, 0, n2.Length);
                obj.Divide(n2, val);
                return;
            }
            base.Divide(names, val);
        }
    }
}
