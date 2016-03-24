using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// A tag information container.
    /// </summary>
    public class TagData
    {
        /// <summary>
        /// What debug mode to use while filling tags.
        /// </summary>
        public DebugMode mode;

        int cInd = 0;

        /// <summary>
        /// The tags current simplified input data.
        /// </summary>
        public string[] InputKeys = null;

        /// <summary>
        /// All 'modifier' data (EG, input[modifier].input[modifer]).
        /// </summary>
        public Argument[] Modifiers = null;

        /// <summary>
        /// What to be returned if the tag fills null.
        /// </summary>
        public Argument Fallback = null;

        /// <summary>
        /// Whether this tag has an alternate response if it fills null.
        /// </summary>
        public bool HasFallback
        {
            get
            {
                return Fallback != null;
            }
        }

        /// <summary>
        /// All variables waiting in this tag's context.
        /// </summary>
        public Dictionary<string, TemplateObject> Variables = null;

        /// <summary>
        /// The tag system this tag data is used for.
        /// </summary>
        public TagParser TagSystem;

        /// <summary>
        /// The 'base color' set by the tag requesting code.
        /// </summary>
        public string BaseColor = null;

        /// <summary>
        /// What to invoke if there is an error. Given string contains valid tags - any user input should be escaped!
        /// </summary>
        public Action<string> Error;

        /// <summary>
        /// Constructs the tag information container.
        /// </summary>
        /// <param name="_system">The command system to use.</param>
        /// <param name="_input">The input tag pieces.</param>
        /// <param name="_basecolor">The default color to use for output.</param>
        /// <param name="_vars">Any variables involved in the queue.</param>
        /// <param name="_mode">What debug mode to use.</param>
        /// <param name="_error">What to invoke if there is an error.</param>
        /// <param name="fallback">What to fall back to if the tag returns null.</param>
        public TagData(TagParser _system, List<TagBit> _input, string _basecolor, Dictionary<string, TemplateObject> _vars, DebugMode _mode, Action<string> _error, Argument fallback)
        {
            TagSystem = _system;
            BaseColor = _basecolor ?? "^r^7";
            Variables = _vars ?? new Dictionary<string, TemplateObject>();
            mode = _mode;
            Error = _error;
            Fallback = fallback;
            Remaining = _input.Count;
            // TODO: Store TagBit list directly?
            if (_input == null)
            {
                InputKeys = new string[0];
                Modifiers = new Argument[0];
            }
            else
            {
                InputKeys = new string[_input.Count];
                Modifiers = new Argument[_input.Count];
                for (int i = 0; i < _input.Count; i++)
                {
                    InputKeys[i] = _input[i].Key;
                    Modifiers[i] = _input[i].Variable ?? new Argument();
                }
            }
        }

        /// <summary>
        /// Constructs the tag information container.
        /// </summary>
        /// <param name="_system">The command system to use.</param>
        /// <param name="_input">The input tag pieces.</param>
        /// <param name="_basecolor">The default color to use for output.</param>
        /// <param name="_vars">Any variables involved in the queue.</param>
        /// <param name="_mode">What debug mode to use.</param>
        /// <param name="wasquoted">Whether the input was quoted.</param>
        /// <param name="_error">What to invoke if there is an error.</param>
        /// <param name="fallback">What to fall back to if the tag returns null.</param>
        public TagData(TagParser _system, List<string> _input, string _basecolor, Dictionary<string, TemplateObject> _vars, DebugMode _mode, Action<string> _error, bool wasquoted, Argument fallback)
        {
            TagSystem = _system;
            InputKeys = new string[_input.Count];
            BaseColor = _basecolor;
            Variables = _vars;
            mode = _mode;
            Error = _error;
            Fallback = fallback;
            Modifiers = new Argument[_input.Count];
            Remaining = _input.Count;
            for (int x = 0; x < _input.Count; x++)
            {
                if (_input[x].Length > 1 && _input[x].Contains('[') && _input[x][_input[x].Length - 1] == ']')
                {
                    int index = _input[x].IndexOf('[');
                    Modifiers[x] = TagSystem.SplitToArgument(_input[x].Substring(index + 1, _input[x].Length - (index + 2)), wasquoted);
                    InputKeys[x] = _input[x].Substring(0, index).ToLowerInvariant();
                }
                else
                {
                    InputKeys[x] = _input[x].ToLowerInvariant();
                    Modifiers[x] = new Argument();
                }
            }
        }

        /// <summary>
        /// Gets the key at a specified index.
        /// </summary>
        /// <param name="ind">The index.</param>
        /// <returns>The key.</returns>
        public string this[int ind]
        {
            get
            {
                return InputKeys[ind + cInd];
            }
        }

        /// <summary>
        /// Shrinks the data amount by one at the start, and returns itself.
        /// </summary>
        /// <returns>This object.</returns>
        public TagData Shrink()
        {
            cInd++;
            Remaining--;
            return this;
        }

        /// <summary>
        /// How many tag positions are left.
        /// </summary>
        public int Remaining;

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within - returning a string.
        /// </summary>
        /// <param name="place">What place to get a modifier from.</param>
        /// <returns>The tag-parsed modifier as a string.</returns>
        public string GetModifier(int place)
        {
            place += cInd;
            if (place < 0 || place >= Modifiers.Length)
            {
                throw new ArgumentOutOfRangeException("place");
            }
            return (Modifiers[place].Parse(BaseColor, Variables, mode, Error) ?? new TextTag("")).ToString();
        }

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What place to get a modifier from.</param>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObject(int place)
        {
            place += cInd;
            if (place < 0 || place >= Modifiers.Length)
            {
                throw new ArgumentOutOfRangeException("place");
            }
            return (Modifiers[place].Parse(BaseColor, Variables, mode, Error) ?? new TextTag(""));
        }
    }
}
