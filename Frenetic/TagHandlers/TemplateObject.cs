using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers
{
    /// <summary>
    /// The abstract base for a tag object.
    /// </summary>
    public abstract class TemplateObject
    {
        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data</param>
        public abstract string Handle(TagData data);
    }
}
