//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
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
        public void Ready(TagHandler tags)
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
