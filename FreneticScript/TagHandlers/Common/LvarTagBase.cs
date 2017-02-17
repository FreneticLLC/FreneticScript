using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles internal compiled var tags.
    /// </summary>
    public class LvarTagBase : TemplateTagBase
    {
        // No meta: compiled only.

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
        /// <returns>The result.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return (data.CSE as CompiledCommandStackEntry).LocalVariables[IntegerTag.For(data, data.GetModifierObject(0)).Internal].Internal;
        }
    }
}
