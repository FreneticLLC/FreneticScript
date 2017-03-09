using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Represents the specific type of a tag.
    /// </summary>
    public class TagType
    {
        /// <summary>
        /// The name of this tag type, lowercase.
        /// </summary>
        public string TypeName;

        /// <summary>
        /// The name of the type upon which this tag type is based.
        /// </summary>
        public string SubTypeName;

        /// <summary>
        /// The raw C# / .NET type of the tag type.
        /// </summary>
        public Type RawType;

        /// <summary>
        /// The type upon which this tag type is based.
        /// </summary>
        public TagType SubType;

        /// <summary>
        /// The method that creates this tag. Set automatically based on raw type.
        /// </summary>
        public MethodInfo CreatorMethod = null;

        /// <summary>
        /// This function should take the two inputs and return a valid object of the relevant type.
        /// TODO: Remove in favor of CreatorMethod!
        /// </summary>
        public Func<TemplateObject, TagData, TemplateObject> TypeGetter;

        /// <summary>
        /// The tag sub-handler for all possible tags.
        /// </summary>
        public Dictionary<string, TagSubHandler> SubHandlers;

        /// <summary>
        /// Contains a mapping of tag names to their helper data. Set automatically based on raw type.
        /// </summary>
        public Dictionary<string, TagHelpInfo> TagHelpers;

        /// <summary>
        /// Gets the object of the next type down the tree of types.
        /// REQUIRES a PUBLIC STATIC METHOD be referenced!
        /// </summary>
        public Func<TemplateObject, TemplateObject> GetNextTypeDown;
    }
}
