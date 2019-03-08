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
using FreneticScript.TagHandlers.Objects;
using System.Reflection;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The abstract base for a tag object.
    /// </summary>
    public abstract class TemplateObject
    {
        /// <summary>
        /// Returns the input as-is, for use with Object Types.
        /// </summary>
        /// <param name="obj">The object input.</param>
        /// <returns>The object input.</returns>
        public static TemplateObject Basic_For(TemplateObject obj)
        {
            return obj;
        }

        /// <summary>
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public abstract string GetTagTypeName();

        /// <summary>
        /// Return the type of this tag.
        /// </summary>
        /// <returns>The tag type.</returns>
        public virtual TagType GetTagType(TagTypes tagTypeSet)
        {
            return tagTypeSet.TypeForName(GetTagTypeName());
        }

        /// <summary>
        /// The symbol that connects a type to it's savable data.
        /// </summary>
        public const string SAVE_MARK = "@";

        /// <summary>
        /// Gets the savable string for this instance, including any relevant type information.
        /// </summary>
        /// <returns>The save string.</returns>
        public virtual string GetSavableString()
        {
            return GetTagTypeName() + SAVE_MARK + ToString();
        }

        /// <summary>
        /// Gets a "clean" text form of an object for simpler output to debug logs, may have added colors or other details.
        /// </summary>
        /// <returns>The debug-friendly string.</returns>
        public virtual string GetDebugString()
        {
            return ToString();
        }
    }
}
