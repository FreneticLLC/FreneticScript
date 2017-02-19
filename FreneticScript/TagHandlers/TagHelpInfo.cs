using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Reflection;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// Helps represent a tag.
    /// </summary>
    public class TagHelpInfo
    {
        /// <summary>
        /// The tag meta data.
        /// </summary>
        public TagMeta Meta;

        /// <summary>
        /// The relevant method.
        /// </summary>
        public MethodInfo Method;
        
        /// <summary>
        /// Constructs the TagHelpInfo.
        /// </summary>
        /// <param name="_method">The method to construct from.</param>
        public TagHelpInfo(MethodInfo _method)
        {
            Method = _method;
            Meta = _method.GetCustomAttribute<TagMeta>();
        }
    }
}
