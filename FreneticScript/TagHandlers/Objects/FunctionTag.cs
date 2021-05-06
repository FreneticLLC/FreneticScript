//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a Function as a tag.
    /// </summary>
    [ObjectMeta(Name = FunctionTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Command System", Description = "Represents a function.")]
    public class FunctionTag : TemplateObject
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
            return tagTypeSet.Type_Function;
        }

        /// <summary>
        /// The represented function.
        /// </summary>
        public CommandScript Internal;

        /// <summary>
        /// Constructs a new FunctionTag.
        /// </summary>
        /// <param name="script">The CommandScript to base this FunctionTag off of.</param>
        public FunctionTag(CommandScript script)
        {
            Internal = script;
        }

        /// <summary>
        /// Returns a FunctionTag for the given text.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="input">The input text.</param>
        /// <returns>A TagTypeTag.</returns>
        public static FunctionTag For(TagData data, string input)
        {
            if (!input.Contains('|'))
            {
                CommandScript script = data.Engine.GetFunction(input);
                if (script == null)
                {
                    throw data.Error($"Unknown script name '{TextStyle.Separate}{input}{TextStyle.Base}'.");
                }
                return new FunctionTag(script);
            }
            ListTag list = ListTag.For(input);
            if (list.Internal.Count < 2)
            {
                throw data.Error("Cannot construct FunctionTag with empty input.");
            }
            string type = list.Internal[0].ToString();
            if (type == "anon")
            {
                if (list.Internal.Count < 4)
                {
                    throw data.Error("Cannot construct FunctionTag without start line, name, and commandlist input.");
                }
                string name = list.Internal[1].ToString();
                int startLine = (int)IntegerTag.For(list.Internal[2], data).Internal;
                string commands = list.Internal[3].ToString();
                CommandScript script = ScriptParser.SeparateCommands(name, commands, data.Engine, startLine, data.DBMode);
                if (script == null)
                {
                    throw data.Error("Anonymous function failed to generate.");
                }
                script.TypeName = CommandScript.TYPE_NAME_ANONYMOUS;
                script.IsAnonymous = true;
                script.AnonymousString = input.Substring("anon|".Length);
                return new FunctionTag(script);
            }
            else if (type == "named")
            {
                string scriptName = list.Internal[1].ToString();
                CommandScript script = data.Engine.GetFunction(scriptName);
                if (script == null)
                {
                    throw data.Error($"Unknown script name '{TextStyle.Separate}{scriptName}{TextStyle.Base}'.");
                }
                return new FunctionTag(script);
            }
            throw data.Error($"Unknown Function type '{TextStyle.Separate}{type}{TextStyle.Base}'.");
        }
        
        /// <summary>
        /// Creates a FunctionTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The input object.</param>
        /// <returns>A valid FunctionTag.</returns>
        public static FunctionTag CreateFor(TemplateObject input, TagData dat)
        {
            return input switch
            {
                FunctionTag ftag => ftag,
                DynamicTag dtag => CreateFor(dtag.Internal, dat),
                _ => For(dat, input.ToString()),
            };
        }

        /// <summary>
        /// The FunctionTag type.
        /// </summary>
        public const string TYPE = "function";

        #pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FunctionTag Tag_Duplicate(FunctionTag obj, TagData data)
        {
            return new FunctionTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (FunctionTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(FunctionTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Types.Type_Function);
        }

#pragma warning restore 1591

        /// <summary>
        /// Gets a simple name for this function.
        /// </summary>
        /// <returns>The name.</returns>
        public string Name()
        {
            if (Internal.IsAnonymous)
            {
                return "anonymous:" + Internal.Name;
            }
            else
            {
                return Internal.Name;
            }
        }

        /// <summary>
        /// Returns the function data.
        /// </summary>
        /// <returns>The function.</returns>
        public override string ToString()
        {
            if (Internal.IsAnonymous)
            {
                return "anon|" + Internal.AnonymousString;
            }
            else
            {
                return "named|" + EscapeTagBase.Escape(Internal.Name);
            }
        }

        /// <summary>
        /// Gets a "clean" text form of an object for simpler output to debug logs, may have added colors or other details.
        /// </summary>
        /// <returns>The debug-friendly string.</returns>
        public override string GetDebugString()
        {
            return TextStyle.Minor + Internal.TypeName + "|" + TextStyle.Separate + Internal.Name;
        }
    }
}
