using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.CommandSystem.Arguments;

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
            return input is MapTag ? (MapTag)input : For(input.ToString());
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
                // @Name MapTag.size
                // @Group Map Entries
                // @ReturnType IntegerTag
                // @Returns the number of entries in the map.
                // @Example "one:a|two:b" .size returns "2".
                // -->
                case "size":
                    {
                        return new IntegerTag(Internal.Count).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name MapTag.keys
                // @Group Map Entries
                // @ReturnType ListTag
                // @Returns a list of all keys in the map.
                // @Example "one:a|two:b" .get[one] returns "one|two|".
                // -->
                case "keys":
                    {
                        ListTag list = new ListTag(Internal.Keys.ToList());
                        return list.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name MapTag.values
                // @Group Map Entries
                // @ReturnType ListTag
                // @Returns a list of all values in the map.
                // @Example "one:a|two:b" .get[one] returns "a|b|".
                // -->
                case "values":
                    {
                        ListTag list = new ListTag();
                        list.ListEntries.AddRange(Internal.Values);
                        return list.Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name MapTag.contains[<TextTag>]
                // @Group Map Entries
                // @ReturnType <Dynamic>
                // @Returns whether the specified entry exists in the map.
                // @Example "one:a|two:b" .contains[one] returns "true".
                // @Example "one:a|two:b" .contains[three] returns "false".
                // -->
                case "contains":
                    {
                        string modif = data.GetModifier(0).ToLower();
                        return new BooleanTag(Internal.ContainsKey(modif)).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name MapTag.get[<TextTag>]
                // @Group Map Entries
                // @ReturnType <Dynamic>
                // @Returns the specified entry value in the map.
                // @Example "one:a|two:b" .get[one] returns "a".
                // @Example "one:a|two:b" .get[two] returns "b".
                // -->
                case "get":
                    {
                        string modif = data.GetModifier(0).ToLower();
                        TemplateObject outp;
                        if (Internal.TryGetValue(modif, out outp))
                        {
                            return outp.Handle(data.Shrink());
                        }
                        data.Error("Unknown map entry: '" + TagParser.Escape(modif) + "'!");
                        return new NullTag();
                    }
                // Documented in TextTag.
                case "duplicate":
                    return new MapTag(Internal).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
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
            TemplateObject obj;
            if (names != null && names.Length > 1 && Internal.TryGetValue(names[0].ToLowerFast(), out obj))
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
            TemplateObject obj;
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out obj))
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
            TemplateObject obj;
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out obj))
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
            TemplateObject obj;
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out obj))
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
            TemplateObject obj;
            if (names != null && names.Length > 0 && Internal.TryGetValue(names[0].ToLowerFast(), out obj))
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
