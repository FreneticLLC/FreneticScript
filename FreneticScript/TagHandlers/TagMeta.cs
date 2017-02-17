using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Represents inline tag meta.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TagMeta : Attribute
    {
        /// <summary>
        /// The type of the tag.
        /// </summary>
        public string TagType;

        /// <summary>
        /// The name of the tag.
        /// </summary>
        public string Name;

        /// <summary>
        /// The group of the tag.
        /// </summary>
        public string Group;

        /// <summary>
        /// The return type of the tag.
        /// </summary>
        public string ReturnType;

        /// <summary>
        /// The return value of the tag.
        /// </summary>
        public string Returns;

        /// <summary>
        /// The internal tag type of this tag.
        /// </summary>
        public TagType ActualType;

        /// <summary>
        /// The internal return tag type of this tag.
        /// </summary>
        public TagType ReturnTypeResult;

        /// <summary>
        /// Prepares the tag meta.
        /// </summary>
        /// <param name="tags">The tag parser.</param>
        public void Ready(TagParser tags)
        {
            ActualType = tags.Types[TagType];
            if (!tags.Types.TryGetValue(ReturnType, out ReturnTypeResult))
            {
                ReturnTypeResult = null;
            }
        }

        /// <summary>
        /// The examples for the tag.
        /// </summary>
        public string[] Examples;

        /// <summary>
        /// Other data for the tag.
        /// </summary>
        public string[] Others;

        /// <summary>
        /// Returns a perfect duplicate of this meta.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public TagMeta Duplicate()
        {
            return MemberwiseClone() as TagMeta;
        }
    }
}
