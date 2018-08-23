//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2018 Frenetic LLC under the terms of a strict license.
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
        /// A field reference to the <see cref="SIMPLE_ERROR"/> field.
        /// </summary>
        public static readonly FieldInfo FIELD_TAGDATA_SIMPLE_ERROR = typeof(TagData).GetField(nameof(SIMPLE_ERROR), BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// A simple error TagData object.
        /// </summary>
        public static readonly TagData SIMPLE_ERROR = GenerateSimpleErrorTagData();
        
        /// <summary>
        /// Generates a simple error tag data instance that can be repurposed.
        /// </summary>
        public static TagData GenerateSimpleErrorTagData()
        {
            return new TagData(null, new Argument[0], new TagBit[0], null, DebugMode.FULL, (s) => throw new Exception("Script error occured: " + s), null, null);
        }

        /// <summary>
        /// Returns a shallow duplicate of this object.
        /// </summary>
        /// <returns>The shallow duplicate.</returns>
        public TagData DuplicateShallow()
        {
            return MemberwiseClone() as TagData;
        }

        /// <summary>
        /// The <see cref="Start"/> field.
        /// </summary>
        public static FieldInfo Field_Start = typeof(TagData).GetField(nameof(Start));
        
        /// <summary>
        /// The <see cref="cInd"/> field.
        /// </summary>
        public static FieldInfo Field_cInd = typeof(TagData).GetField(nameof(cInd));

        /// <summary>
        /// The start of this data.
        /// </summary>
        public TemplateTagBase Start;

        /// <summary>
        /// What debug mode to use while filling tags.
        /// </summary>
        public DebugMode DBMode;

        /// <summary>
        /// The current index in this tag.
        /// </summary>
        public int cInd = 0;

        /// <summary>
        /// The tag's current variable arguments.
        /// </summary>
        public Argument[] Variables = null;

        /// <summary>
        /// The tag bits this tag data sources from.
        /// </summary>
        public TagBit[] Bits = null;
        
        /// <summary>
        /// What to be returned if the tag fills null.
        /// </summary>
        public Argument Fallback = null;

        /// <summary>
        /// The source argument ID within the command.
        /// </summary>
        public string SourceArgumentID = "<none>";

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
        /// Gets the command system this tag data is used for (gathered from <see cref="TagSystem"/>).
        /// </summary>
        public Commands CommandSystem
        {
            get
            {
                return TagSystem.CommandSystem;
            }
        }

        /// <summary>
        /// The 'base color' set by the tag requesting code.
        /// </summary>
        public string BaseColor = null;

        /// <summary>
        /// What to invoke if there is an error. Used for setting a resultant call. If an error is encountered in a tag, use <see cref="Error(string)"/>.
        /// </summary>
        public Action<string> ErrorHandler;

        /// <summary>
        /// Call to start the error handling system.
        /// </summary>
        /// <param name="message">Error message.</param>
        public void Error(string message)
        {
            RunError(message);
        }

        /// <summary>
        /// Callable action for handling errors.
        /// </summary>
        public Action<string> ErrorAction;

        /// <summary>
        /// The relevant command stack entry, if any.
        /// </summary>
        public CompiledCommandStackEntry CSE;

        /// <summary>
        /// Runs a clean error for this tag.
        /// </summary>
        /// <param name="message">The error message.</param>
        public void RunError(string message)
        {
            ErrorHandler(message + "\n    while handling tag "
                + TextStyle.Color_Separate + "<" + HighlightString(cInd, TextStyle.Color_Warning)
                + TextStyle.Color_Separate + ">" + TextStyle.Color_Base + " under sub-tag '"
                + TextStyle.Color_Separate + Bits[cInd].ToString() + TextStyle.Color_Base + "' for type '"
                + TextStyle.Color_Separate + Bits[cInd].TagHandler.Meta.ActualType.TypeName + TextStyle.Color_Base + "' at index "
                + TextStyle.Color_Separate + cInd + TextStyle.Color_Base + " in command argument "
                + TextStyle.Color_Separate + SourceArgumentID + TextStyle.Color_Base);
        }

        /// <summary>
        /// Returns the tag as tag input text, highlighting a specific index. Does not include wrapping tag marks.
        /// </summary>
        /// <param name="index">The index to highlight at.</param>
        /// <param name="highlight">The highlight string.</param>
        /// <returns>Tag input text.</returns>
        public string HighlightString(int index, string highlight)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Bits.Length; i++)
            {
                if (i == index)
                {
                    sb.Append(highlight);
                }
                sb.Append(Bits[i].ToString());
                if (i + 1 < Bits.Length)
                {
                    sb.Append(".");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Constructs an empty unfilled tag data (FILL THIS OBJECT AFTER USING THIS).
        /// </summary>
        public TagData()
        {
            ErrorAction = Error;
            // Assume the Tag system will fill vars.
        }

        /// <summary>
        /// Constructs the tag information container.
        /// </summary>
        /// <param name="_system">The command system to use.</param>
        /// <param name="_vars">The variable argument pieces.</param>
        /// <param name="_bits">The tag bits.</param>
        /// <param name="_basecolor">The default color to use for output.</param>
        /// <param name="_mode">What debug mode to use.</param>
        /// <param name="_error">What to invoke if there is an error.</param>
        /// <param name="fallback">What to fall back to if the tag returns null.</param>
        /// <param name="_cse">The relevant command stack entry, if any.</param>
        public TagData(TagParser _system, Argument[] _vars, TagBit[] _bits, string _basecolor, DebugMode _mode, Action<string> _error, Argument fallback, CompiledCommandStackEntry _cse)
        {
            TagSystem = _system;
            BaseColor = _basecolor ?? TextStyle.Color_Simple;
            DBMode = _mode;
            ErrorHandler = _error;
            Fallback = fallback;
            Variables = _vars;
            Bits = _bits;
            CSE = _cse;
            ErrorAction = Error;
        }

        /// <summary>
        /// How many tag positions are left.
        /// </summary>
        public int Remaining
        {
            get
            {
                return Bits.Length - cInd;
            }
        }
        
        /// <summary>
        /// Gets the modifier at the current position, handling any tags within - returning a string.
        /// </summary>
        /// <returns>The tag-parsed modifier as a string.</returns>
        public string GetModifierCurrent()
        {
            return GetModifierObjectCurrent().ToString();
        }

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
            return Variables[place].Parse(ErrorAction, CSE);
        }

        /// <summary>
        /// Gets the modifier at the current position, handling any tags within.
        /// </summary>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObjectCurrent()
        {
            return Variables[cInd].Parse(ErrorAction, CSE);
        }

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What place to get a modifier from (relative to current index).</param>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObject(int place)
        {
            return Variables[place + cInd].Parse(ErrorAction, CSE);
        }
    }
}
