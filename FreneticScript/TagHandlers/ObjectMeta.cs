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
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Represents tag object type meta.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ObjectMeta : ScriptMetaAttribute
    {
        /// <summary>
        /// The name of the tag type.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// The internal tag type.
        /// </summary>
        public TagType ActualType;

        /// <summary>
        /// The name of the sub-type of this type.
        /// </summary>
        public string SubTypeName;

        /// <summary>
        /// The internal sub-type of this type.
        /// </summary>
        public TagType ActualSubType;
        
        /// <summary>
        /// A description of this tag type.
        /// </summary>
        public string Description;
        
        /// <summary>
        /// Prepares the object meta.
        /// </summary>
        /// <param name="tags">The tag parser.</param>
        public void Ready(TagParser tags)
        {
            if (Name == null || !tags.Types.RegisteredTypes.TryGetValue(Name, out ActualType))
            {
                 tags.CommandSystem.Context.BadOutput("Cannot register object meta for type " + Name + " because that type does not exist.");
            }
            if (SubTypeName == null || !tags.Types.RegisteredTypes.TryGetValue(SubTypeName, out ActualSubType))
            {
                ActualSubType = null;
            }
        }
    }
}
