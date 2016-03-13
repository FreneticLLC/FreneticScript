using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;

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
            Internal = toUse;
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
            string[] dat = input.Split('|');
            MapTag map = new MapTag();
            for (int i = 0; i < dat.Length; i++)
            {
                string[] kvp = dat[i].Split(':');
                if (kvp.Length != 2)
                {
                    // TODO: Error?
                    continue;
                }
                map.Internal[kvp[0].ToLower()] = new TextTag(kvp[1]);
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
            if (data.Input.Count == 0)
            {
                return this;
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name MapTag.size
                // @Group Map Entries
                // @ReturnType IntegerTag
                // @Returns the specified entry value in the map.
                // @Other note that indices are one-based.
                // @Example "one:a|two:b" .get[one] returns "a".
                // @Example "one:a|two:b" .get[two] returns "b".
                // -->
                case "size":
                    {
                        return new IntegerTag(Internal.Count).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name MapTag.contains[<TextTag>]
                // @Group Map Entries
                // @ReturnType Dynamic
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
                // @ReturnType Dynamic
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
    }
}
