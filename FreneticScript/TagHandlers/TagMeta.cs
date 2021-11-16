//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;

namespace FreneticScript.TagHandlers
{
    /// <summary>Represents inline tag meta.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TagMeta : ScriptMetaAttribute
    {
        /// <summary>The type of the tag.</summary>
        public string TagType;

        /// <summary>The name of the tag.</summary>
        public string Name;

        /// <summary>The return type of the tag.</summary>
        public string ReturnType;

        /// <summary>The return value of the tag.</summary>
        public string Returns;

        /// <summary>The internal tag type of this tag.</summary>
        public TagType ActualType;

        /// <summary>The internal return tag type of this tag.</summary>
        public TagReturnType ReturnTypeResult;

        /// <summary>The modifier type of this tag.</summary>
        public string Modifier;

        /// <summary>The internal modifier tag type of this tag.</summary>
        public TagReturnType ModifierType;

        /// <summary>Indicates that this tag is to be treated as a special self-compiler tag.</summary>
        public bool SpecialCompiler;

        /// <summary>Indicates whether the object self-input should be in raw form.</summary>
        public bool SelfIsRaw;

        /// <summary>The special compiler callable for this tag, if marked with <see cref="SpecialCompiler"/>.</summary>
        public Func<ILGeneratorTracker, TagArgumentBit, int, TagReturnType, TagReturnType> SpecialCompileAction;

        /// <summary>
        /// The method name of the type helper for this tag, if any.
        /// Must be a static method within the same class as the tag method itself.
        /// </summary>
        public string SpecialTypeHelperName;

        /// <summary>The special type helper callable for this tag, if named by <see cref="SpecialTypeHelperName"/>.</summary>
        public Func<TagArgumentBit, int, TagReturnType> SpecialTypeHelper;

        /// <summary>Prepares the tag meta.</summary>
        /// <param name="tags">The tag parser.</param>
        public void Ready(TagHandler tags)
        {
            ActualType = TagType == null ? null : tags.Types.RegisteredTypes[TagType];
            if (ReturnType == null || !tags.Types.RegisteredTypes.TryGetValue(ReturnType, out TagType returnedTagType))
            {
                ReturnTypeResult = default;
            }
            else
            {
                ReturnTypeResult = new TagReturnType(returnedTagType, false);
            }
            ModifierType = new TagReturnType(Modifier == null ? null : tags.Types.RegisteredTypes[Modifier], false);
        }

        /// <summary>The examples for the tag.</summary>
        public string[] Examples;

        /// <summary>Returns a perfect duplicate of this meta.</summary>
        /// <returns>The duplicate.</returns>
        public TagMeta Duplicate()
        {
            return MemberwiseClone() as TagMeta;
        }
    }
}
