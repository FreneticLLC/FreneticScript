//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem;
using System.Reflection;

namespace FreneticScript.TagHandlers.HelperBases
{
    /// <summary>
    /// Handles internal compiled var tags.
    /// </summary>
    public class LvarTagBase : TemplateTagBase
    {
        // No meta: compiled only.

        /// <summary>
        /// The 'HandleOneFast' method, for compilation use.
        /// </summary>
        public static MethodInfo Method_HandleOneFast = typeof(LvarTagBase).GetMethod("HandleOneFast", BindingFlags.Public | BindingFlags.Static);

        /// <summary>
        /// Construct the Lvar tag base.
        /// </summary>
        public LvarTagBase()
        {
            Name = "\0lvar";
        }
        
        /// <summary>
        /// Handles a single entry.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="loc">The location of the variable.</param>
        /// <returns>The result.</returns>
        public static TemplateObject HandleOneFast(TagData data, int loc)
        {
            return data.CSE.LocalVariables[loc].Internal;
        }

        /// <summary>
        /// Handles a single entry.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The result.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            throw new NotImplementedException("LVar was called incorrectly!");
        }
    }
}
