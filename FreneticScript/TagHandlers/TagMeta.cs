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
using System.Threading.Tasks;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Represents inline tag meta.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TagMeta : Attribute
    {
        /// <summary>
        /// The type of the tag.
        /// </summary>
        public string TagType;

        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name;

        /// <summary>
        /// The group of the tag.
        /// </summary>
        public string Group;

        /// <summary>
        /// The return type of the tag.
        /// </summary>
        public string ReturnType;

        /// <summary>
        /// The return value of the tag.
        /// </summary>
        public string Returns;

        /// <summary>
        /// The internal tag type of this tag.
        /// </summary>
        public TagType ActualType;

        /// <summary>
        /// The internal return tag type of this tag.
        /// </summary>
        public TagType ReturnTypeResult;

        /// <summary>
        /// The modifier type of this tag.
        /// </summary>
        public string Modifier;

        /// <summary>
        /// The internal modifier tag type of this tag.
        /// </summary>
        public TagType ModifierType;

        /// <summary>
        /// Prepares the tag meta.
        /// </summary>
        /// <param name="tags">The tag parser.</param>
        public void Ready(TagParser tags)
        {
            ActualType = TagType == null ? null : tags.Types[TagType];
            if (ReturnType == null || !tags.Types.TryGetValue(ReturnType, out ReturnTypeResult))
            {
                ReturnTypeResult = null;
            }
            ModifierType = Modifier == null ? null : tags.Types[Modifier];
        }

        /// <summary>
        /// The examples for the tag.
        /// </summary>
        public string[] Examples;

        /// <summary>
        /// Other data for the tag.
        /// </summary>
        public string[] Others;

        /// <summary>
        /// Returns a perfect duplicate of this meta.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public TagMeta Duplicate()
        {
            return MemberwiseClone() as TagMeta;
        }
    }
}
