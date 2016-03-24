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

        /// <summary>
        /// The current index in this tag.
        /// </summary>
        public int cInd = 0;

        /// <summary>
        /// The tag's current simplified input data.
        /// </summary>
        public TagBit[] InputKeys = null;
        
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
        public TagData(TagParser _system, TagBit[] _input, string _basecolor, Dictionary<string, TemplateObject> _vars, DebugMode _mode, Action<string> _error, Argument fallback)
        {
            TagSystem = _system;
            BaseColor = _basecolor ?? "^r^7";
            Variables = _vars ?? new Dictionary<string, TemplateObject>();
            mode = _mode;
            Error = _error;
            Fallback = fallback;
            Remaining = _input.Length;
            InputKeys = _input;
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
            InputKeys = new TagBit[_input.Count];
            BaseColor = _basecolor;
            Variables = _vars;
            mode = _mode;
            Error = _error;
            Fallback = fallback;
            Remaining = _input.Count;
            for (int x = 0; x < _input.Count; x++)
            {
                InputKeys[x] = new TagBit();
                if (_input[x].Length > 1 && _input[x].Contains('[') && _input[x][_input[x].Length - 1] == ']')
                {
                    int index = _input[x].IndexOf('[');
                    InputKeys[x].Key = _input[x].Substring(0, index).ToLowerInvariant();
                    InputKeys[x].Variable = TagSystem.SplitToArgument(_input[x].Substring(index + 1, _input[x].Length - (index + 2)), wasquoted);
                }
                else
                {
                    InputKeys[x].Key = _input[x].ToLowerInvariant();
                    InputKeys[x].Variable = new Argument();
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
                return InputKeys[ind + cInd].Key;
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
            if (place < 0 || place >= InputKeys.Length)
            {
                throw new ArgumentOutOfRangeException("place");
            }
            return (InputKeys[place].Variable.Parse(BaseColor, Variables, mode, Error) ?? new TextTag("")).ToString();
        }

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What place to get a modifier from.</param>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObject(int place)
        {
            place += cInd;
            if (place < 0 || place >= InputKeys.Length)
            {
                throw new ArgumentOutOfRangeException("place");
            }
            return (InputKeys[place].Variable.Parse(BaseColor, Variables, mode, Error) ?? new TextTag(""));
        }
    }
}
