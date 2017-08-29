//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// A tag information container.
    /// </summary>
    public class TagData
    {
        /// <summary>
        /// The "Start" field.
        /// </summary>
        public static FieldInfo Field_Start = typeof(TagData).GetField("Start");

        /// <summary>
        /// The "InputKeys" field.
        /// </summary>
        public static FieldInfo Field_InputKeys = typeof(TagData).GetField("InputKeys");

        /// <summary>
        /// The "ShrinkMulti" method.
        /// </summary>
        public static MethodInfo Method_ShrinkMulti = typeof(TagData).GetMethod("ShrinkMulti");

        /// <summary>
        /// The "Shrink" method.
        /// </summary>
        public static MethodInfo Method_Shrink = typeof(TagData).GetMethod("Shrink");

        /// <summary>
        /// The start of this data.
        /// </summary>
        public TemplateTagBase Start;

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
        /// The relevant command stack entry, if any.
        /// </summary>
        public CompiledCommandStackEntry CSE;

        /// <summary>
        /// Constructs an empty unfilled tag data (FILL THIS OBJECT AFTER USING THIS).
        /// </summary>
        public TagData()
        {
            // Assume the Tag system will fill vars.
        }

        /// <summary>
        /// Constructs the tag information container.
        /// </summary>
        /// <param name="_system">The command system to use.</param>
        /// <param name="_input">The input tag pieces.</param>
        /// <param name="_basecolor">The default color to use for output.</param>
        /// <param name="_mode">What debug mode to use.</param>
        /// <param name="_error">What to invoke if there is an error.</param>
        /// <param name="fallback">What to fall back to if the tag returns null.</param>
        /// <param name="_cse">The relevant command stack entry, if any.</param>
        public TagData(TagParser _system, TagBit[] _input, string _basecolor, DebugMode _mode, Action<string> _error, Argument fallback, CompiledCommandStackEntry _cse)
        {
            TagSystem = _system;
            BaseColor = _basecolor ?? TextStyle.Color_Simple;
            mode = _mode;
            Error = _error;
            Fallback = fallback;
            Remaining = _input.Length;
            InputKeys = _input;
            CSE = _cse;
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
        /// Shrinks the data amount by X at the start, and returns itself.
        /// </summary>
        /// <returns>This object.</returns>
        public void ShrinkMulti(int x)
        {
            cInd += x;
            Remaining -= x;
        }

        /// <summary>
        /// Shrinks the data amount by one at the start, and returns itself.
        /// </summary>
        /// <returns>This object.</returns>
        public void Shrink()
        {
            cInd++;
            Remaining--;
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
            return GetModifierObject(place).ToString();
        }

        /// <summary>
        /// Reference to <see cref="GetModifierObjectKnown(int)"/>.
        /// </summary>
        public static MethodInfo Method_GetModiferObjectKnown = typeof(TagData).GetMethod("GetModifierObjectKnown");

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What precise place to get a modifier from.</param>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObjectKnown(int place)
        {
            return InputKeys[place].Variable.Parse(Error, CSE);
        }

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What place to get a modifier from.</param>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObject(int place)
        {
            return InputKeys[place + cInd].Variable.Parse(Error, CSE);
        }
    }
}
