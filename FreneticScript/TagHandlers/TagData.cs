//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.ScriptSystems;
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
        /// A field reference to the <see cref="Runnable"/> field.
        /// </summary>
        public static readonly FieldInfo Field_TagData_Runnable = typeof(TagData).GetField(nameof(Runnable));

        /// <summary>
        /// A simple error TagData object.
        /// </summary>
        public static readonly TagData SIMPLE_ERROR = GenerateSimpleErrorTagData();
        
        /// <summary>
        /// Generates a simple error tag data instance that can be repurposed.
        /// </summary>
        public static TagData GenerateSimpleErrorTagData()
        {
            return new TagData(null, Array.Empty<Argument>(), Array.Empty<TagBit>(), null, DebugMode.FULL, (s) => throw new ErrorInducedException("Script error occured: " + s), null, null);
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
        /// The <see cref="Fallback"/> field.
        /// </summary>
        public static FieldInfo Field_Fallback = typeof(TagData).GetField(nameof(Fallback));

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
        public TagHandler TagSystem;

        /// <summary>
        /// Gets the command system this tag data is used for (gathered from <see cref="TagSystem"/>).
        /// </summary>
        public ScriptEngine Engine
        {
            get
            {
                return TagSystem.Engine;
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
        /// Helper for <see cref="Action"/>s, a no-return call to <see cref="Error(string)"/>.
        /// </summary>
        /// <param name="message">Error message.</param>
        public void ErrorNoReturn(string message)
        {
            Error(message);
        }

        /// <summary>
        /// Call to start the error handling system. Guaranteed exception. Returns an exception to allow 'throw Error(...)' syntax, but is not required (method call will throw exception and never return).
        /// </summary>
        /// <param name="message">Error message.</param>
        public Exception Error(string message)
        {
            PreError();
            RunError(message);
            return new ErrorInducedException("TagData error handler did not throw exception.");
        }

        /// <summary>
        /// Helper call to handle an error case without generating a full error message if available. No guaranteed exit.
        /// </summary>
        public void PreError()
        {
            if (HasFallback)
            {
                throw new TagErrorInducedException();
            }
        }

        /// <summary>
        /// Callable action for handling errors.
        /// </summary>
        public Action<string> ErrorAction;

        /// <summary>
        /// The relevant command runnable, if any.
        /// </summary>
        public CompiledCommandRunnable Runnable;

        /// <summary>
        /// Runs a clean error for this tag.
        /// </summary>
        /// <param name="message">The error message.</param>
        public void RunError(string message)
        {
            if (Bits.Length == 0)
            {
                ErrorHandler(message);
                return;
            }
            ErrorHandler(message + "\n    while handling tag "
                + TextStyle.Separate + "<" + HighlightString(cInd, TextStyle.Warning)
                + TextStyle.Separate + ">" + TextStyle.Base + " under sub-tag '"
                + TextStyle.Separate + Bits[cInd].ToString() + TextStyle.Base
                + (Bits[cInd].TagHandler == null ? "" : "' for type '"
                + TextStyle.Separate + Bits[cInd].TagHandler.Meta.ActualType.TypeName + TextStyle.Base)
                + "' at index " + TextStyle.Separate + cInd + TextStyle.Base + " in command argument "
                + TextStyle.Separate + SourceArgumentID + TextStyle.Base);
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
                    sb.Append('.');
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Constructs an empty unfilled tag data (FILL THIS OBJECT AFTER USING THIS).
        /// </summary>
        public TagData()
        {
            ErrorAction = ErrorNoReturn;
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
        /// <param name="_runnable">The relevant command runnable, if any.</param>
        public TagData(TagHandler _system, Argument[] _vars, TagBit[] _bits, string _basecolor, DebugMode _mode, Action<string> _error, Argument fallback, CompiledCommandRunnable _runnable)
        {
            TagSystem = _system;
            BaseColor = _basecolor ?? TextStyle.Simple;
            DBMode = _mode;
            ErrorHandler = _error;
            Fallback = fallback;
            Variables = _vars;
            Bits = _bits;
            Runnable = _runnable;
            ErrorAction = ErrorNoReturn;
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
        public static MethodInfo Method_GetModiferObjectKnown = typeof(TagData).GetMethod(nameof(GetModifierObjectKnown));

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What precise place to get a modifier from.</param>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObjectKnown(int place)
        {
            return Variables[place].Parse(ErrorAction, Runnable);
        }

        /// <summary>
        /// Reference to <see cref="GetModifierObjectCurrent"/>.
        /// </summary>
        public static MethodInfo Method_GetModifierObjectCurrent = typeof(TagData).GetMethod(nameof(GetModifierObjectCurrent));

        /// <summary>
        /// Gets the modifier at the current position, handling any tags within.
        /// </summary>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObjectCurrent()
        {
            return Variables[cInd].Parse(ErrorAction, Runnable);
        }

        /// <summary>
        /// Gets the modifier at a specified place, handling any tags within.
        /// </summary>
        /// <param name="place">What place to get a modifier from (relative to current index).</param>
        /// <returns>The tag-parsed modifier.</returns>
        public TemplateObject GetModifierObject(int place)
        {
            return Variables[place + cInd].Parse(ErrorAction, Runnable);
        }
    }
}
