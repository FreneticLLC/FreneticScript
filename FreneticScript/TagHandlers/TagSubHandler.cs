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

namespace FreneticScript.TagHandlers
{
    /// <summary>Represents a tag's handler, within the tag type.</summary>
    public class TagSubHandler
    {
        /// <summary>The <see cref="Handle"/> field.</summary>
        public static FieldInfo Field_Handle = typeof(TagSubHandler).GetField(nameof(Handle));

        /// <summary>This function should take the input and output the result of the tag handling.</summary>
        public Func<TagData, TemplateObject, TemplateObject> Handle;

        /// <summary>What type this returns as. Null means dynamic return.</summary>
        public TagType ReturnType = null;

        /// <summary>What type this returns as. Specify null for dynamic return.</summary>
        public string ReturnTypeString = null;

        /// <summary>Whether this tag is for null checks.</summary>
        public bool IsNullTyped = false;

        /// <summary>Returns a perfect duplicate of this tag sub-handler.</summary>
        /// <returns>The duplicate.</returns>
        public TagSubHandler Duplicate()
        {
            return (TagSubHandler)MemberwiseClone();
        }
    }
}
