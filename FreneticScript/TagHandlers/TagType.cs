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
using System.Reflection;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Represents the specific type of a tag.
    /// </summary>
    public class TagType
    {
        /// <summary>
        /// The name of this tag type, lowercase.
        /// </summary>
        public string TypeName;

        /// <summary>
        /// The name of the type upon which this tag type is based.
        /// </summary>
        public string SubTypeName;

        /// <summary>
        /// The raw C# / .NET type of the tag type.
        /// </summary>
        public Type RawType;

        /// <summary>
        /// The type upon which this tag type is based.
        /// </summary>
        public TagType SubType;

        /// <summary>
        /// The method that creates this tag. Set automatically based on raw type.
        /// </summary>
        public MethodInfo CreatorMethod = null;

        /// <summary>
        /// This function should take the two inputs and return a valid object of the relevant type.
        /// TODO: Remove in favor of CreatorMethod!
        /// </summary>
        public Func<TemplateObject, TagData, TemplateObject> TypeGetter;

        /// <summary>
        /// The tag sub-handler for all possible tags.
        /// </summary>
        public Dictionary<string, TagSubHandler> SubHandlers;

        /// <summary>
        /// Contains a mapping of tag names to their helper data. Set automatically based on raw type.
        /// </summary>
        public Dictionary<string, TagHelpInfo> TagHelpers;

        /// <summary>
        /// Gets the object of the next type down the tree of types.
        /// REQUIRES a PUBLIC STATIC METHOD be referenced!
        /// TODO: Replace with direct method referencing (to avoid casting trouble).
        /// </summary>
        public Func<TemplateObject, TemplateObject> GetNextTypeDown;
    }
}
