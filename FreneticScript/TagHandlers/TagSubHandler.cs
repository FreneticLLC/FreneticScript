using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Represents a tag's handler, within the tag type.
    /// </summary>
    public class TagSubHandler
    {
        /// <summary>
        /// This function should take the input and output the result of the tag handling.
        /// </summary>
        public Func<TagData, TemplateObject, TemplateObject> Handle;

        /// <summary>
        /// What type this returns as. Null means dynamic return.
        /// </summary>
        public TagType ReturnType;

        /// <summary>
        /// What type this returns as. Specify null for dynamic return.
        /// </summary>
        public string ReturnTypeString;

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
