using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers
{
    public abstract class TemplateTags
    {
        /// <summary>
        /// The name of the tag set.
        /// </summary>
        public string Name = null;

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data</param>
        public abstract string Handle(TagData data);

        public override string ToString()
        {
            return Name;
        }
    }
}
