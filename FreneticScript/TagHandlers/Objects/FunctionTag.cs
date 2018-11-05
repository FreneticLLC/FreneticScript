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
using System.Runtime.CompilerServices;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;
using FreneticScript.ScriptSystems;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a TagType, as a tag.
    /// </summary>
    public class FunctionTag : TemplateObject
    {
        // <--[object]
        // @Type FunctionTag
        // @SubType TextTag
        // @Group Tag System
        // @Description Represents a function.
        // -->

        /// <summary>
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public override string GetTagTypeName()
        {
            return TYPE;
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
            ListTag list = ListTag.For(input);
            if (list.Internal.Count == 0)
            {
                throw data.Error("Cannot construct FunctionTag with empty input.");
            }
            if (list.Internal.Count == 1)
            {
                string scriptName = list.Internal[0].ToString();
                CommandScript script = data.CommandSystem.GetFunction(scriptName);
                if (script == null)
                {
                    throw data.Error("Unknown script name '" + TextStyle.Color_Separate + scriptName + TextStyle.Color_Base + "'.");
                }
                return new FunctionTag(script);
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
                CommandScript script = ScriptParser.SeparateCommands(name, commands, data.CommandSystem, startLine);
                script.IsAnonymous = true;
                script.AnonymousString = input.Substring("anon|".Length);
                return new FunctionTag(script);
            }
            else if (type == "named")
            {
                string scriptName = list.Internal[1].ToString();
                CommandScript script = data.CommandSystem.GetFunction(scriptName);
                if (script == null)
                {
                    throw data.Error("Unknown script name '" + TextStyle.Color_Separate + scriptName + TextStyle.Color_Base + "'.");
                }
                return new FunctionTag(script);
            }
            throw data.Error("Unknown Function type '" + TextStyle.Color_Separate + type + TextStyle.Color_Base + "'.");
        }
        
        /// <summary>
        /// Creates a FunctionTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The input object.</param>
        /// <returns>A valid FunctionTag.</returns>
        public static FunctionTag CreateFor(TemplateObject input, TagData dat)
        {
            switch (input)
            {
                case FunctionTag ftag:
                    return ftag;
                case DynamicTag dtag:
                    return CreateFor(dtag.Internal, dat);
                default:
                    return For(dat, input.ToString());
            }
        }

        /// <summary>
        /// The FunctionTag type.
        /// </summary>
        public const string TYPE = "functiontag";

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
            return new TagTypeTag(data.TagSystem.Type_Function);
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
    }
}
