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
    /// Represents a tag's handler, within the tag type.
    /// </summary>
    public class TagSubHandler
    {
        /// <summary>
        /// The 'Handle' field.
        /// </summary>
        public static FieldInfo Field_Handle = typeof(TagSubHandler).GetField("Handle");

        /// <summary>
        /// This function should take the input and output the result of the tag handling.
        /// </summary>
        public Func<TagData, TemplateObject, TemplateObject> Handle;

        /// <summary>
        /// What type this returns as. Null means dynamic return.
        /// </summary>
        public TagType ReturnType = null;

        /// <summary>
        /// What type this returns as. Specify null for dynamic return.
        /// </summary>
        public string ReturnTypeString = null;

        /// <summary>
        /// Whether this tag is for null checks.
        /// </summary>
        public bool IsNullTyped = false;

        /// <summary>
        /// Returns a perfect duplicate of this tag sub-handler.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public TagSubHandler Duplicate()
        {
            return (TagSubHandler)MemberwiseClone();
        }
    }
}
