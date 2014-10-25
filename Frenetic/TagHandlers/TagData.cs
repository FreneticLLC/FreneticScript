using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;

namespace Frenetic.TagHandlers
{
    public class TagData
    {
        /// <summary>
        /// What debug mode to use while filling tags.
        /// </summary>
        public DebugMode mode;

        /// <summary>
        /// The tags current simplified input data.
        /// </summary>
        public List<string> Input = null;

        /// <summary>
        /// All 'modifier' data (EG, input[modifier].input[modifer]).
        /// </summary>
        public List<string> Modifiers = null;

        /// <summary>
        /// All variables waiting in this tag's context.
        /// </summary>
        public Dictionary<string, string> Variables = null;

        /// <summary>
        /// The tag system this tag data is used for.
        /// </summary>
        public TagParser TagSystem;

        /// <summary>
        /// The 'base color' set by the tag requesting code.
        /// </summary>
        public string BaseColor = null;

        public TagData(TagParser _system, List<string> _input, string _basecolor, Dictionary<string, string> _vars, DebugMode _mode)
        {
            TagSystem = _system;
            Input = _input;
            BaseColor = _basecolor;
            Variables = _vars;
            mode = _mode;
            Modifiers = new List<string>();
            for (int x = 0; x < Input.Count; x++)
            {
                Input[x] = Input[x].Replace("&dot", ".").Replace("&amp", "&");
                if (Input[x].Length > 1 && Input[x].Contains('[') && Input[x][Input[x].Length - 1] == ']')
                {
                    int index = Input[x].IndexOf('[');
                    Modifiers.Add(Input[x].Substring(index + 1, Input[x].Length - (index + 2)));
                    Input[x] = Input[x].Substring(0, index).ToLower();
                }
                else
                {
                    Input[x] = Input[x].ToLower();
                    Modifiers.Add("");
                }
            }
        }

        /// <summary>
        /// Shrinks the data amount by one at the start, and returns itself.
        /// </summary>
        /// <returns>This object</returns>
        public TagData Shrink()
        {
            if (Input.Count > 0)
            {
                Input.RemoveAt(0);
            }
            if (Modifiers.Count > 0)
            {
                Modifiers.RemoveAt(0);
            }
            return this;
        }

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What place to get a modifier from</param>
        /// <returns>The tag-parsed modifier</returns>
        public string GetModifier(int place)
        {
            if (place < 0 || place >= Modifiers.Count)
            {
                return "";
            }
            return TagSystem.ParseTags(Modifiers[place], TextStyle.Color_Simple, Variables, mode);
        }
    }
}
