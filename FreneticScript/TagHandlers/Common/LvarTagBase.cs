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
            CanSingle = true;
        }

        /// <summary>
        /// Handles a single entry.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The result.</returns>
        public override TemplateObject HandleOne(TagData data)
        {
            IntegerTag itag = IntegerTag.For(data, data.GetModifierObject(0));
            CompiledCommandStackEntry ccse = data.CSE as CompiledCommandStackEntry;
            if (ccse != null)
            {
                return ccse.LocalVariables[itag.Internal].Internal;
            }
            if (!data.HasFallback)
            {
                data.Error("Invalid local variable ID '" + itag.Internal + "'!");
            }
            return new NullTag();
        }
    }
}
