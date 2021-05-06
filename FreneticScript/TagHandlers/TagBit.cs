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
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Part of a tag.
    /// </summary>
    public class TagBit
    {
        /// <summary>
        /// The 'Handler' field.
        /// </summary>
        public static FieldInfo Field_Handler = typeof(TagBit).GetField(nameof(TagBit.Handler));

        /// <summary>
        /// The 'TagHandler' field.
        /// </summary>
        public static FieldInfo Field_TagHandler = typeof(TagBit).GetField(nameof(TagBit.TagHandler));

        /// <summary>
        /// The main portion of the tag.
        /// </summary>
        public string Key = null;

        /// <summary>
        /// The [Modifier] portion of a tag.
        /// </summary>
        public Argument Variable = null;

        /// <summary>
        /// The original input. (For use with compiled data).
        /// </summary>
        public string OriginalInput;

        /// <summary>
        /// The handler that could handle this tag, if any is available.
        /// </summary>
        public TagSubHandler Handler = null;

        /// <summary>
        /// The help info that could handle this tag, if any is available.
        /// </summary>
        public TagHelpInfo TagHandler = null;

        /// <summary>
        /// Returns the tag bit as tag input text.
        /// </summary>
        /// <returns>The tag input text.</returns>
        public override string ToString()
        {
            if (Variable == null || Variable.Bits == null || Variable.Bits.Length == 0)
            {
                return Key;
            }
            else if (Key.StartsWithNull())
            {
                return OriginalInput;
            }
            else
            {
                return Key + "[" + Variable.ToString() + "]";
            }
        }
    }
}
