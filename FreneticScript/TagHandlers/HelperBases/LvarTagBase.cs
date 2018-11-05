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
