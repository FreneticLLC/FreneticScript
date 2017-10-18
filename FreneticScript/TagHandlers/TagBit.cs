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
using FreneticScript.CommandSystem.Arguments;
using System.Reflection;

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
        public static FieldInfo Field_Handler = typeof(TagBit).GetField("Handler");

        /// <summary>
        /// The 'TagHandler' field.
        /// </summary>
        public static FieldInfo Field_TagHandler = typeof(TagBit).GetField("TagHandler");

        /// <summary>
        /// The main portion of the tag.
        /// </summary>
        public string Key = null;

        /// <summary>
        /// The [Modifier] portion of a tag.
        /// </summary>
        public Argument Variable = null;

        /// <summary>
        /// The original input variable. (For use with compiled data).
        /// </summary>
        public Argument OVar = null;

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
            else if (Key.StartsWithNullFS())
            {
                return "[" + OVar.ToString() + "]";
            }
            else
            {
                return Key + "[" + Variable.ToString() + "]";
            }
        }
    }
}
