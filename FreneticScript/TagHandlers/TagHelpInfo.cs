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
        /// TODO: Remove this!
        /// Calls the method directly.
        /// </summary>
        public MethodHandler TEMP_MethodCaller;

        /// <summary>
        /// Represents a method caller function.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <param name="obj">The object to handle.</param>
        public delegate TemplateObject MethodHandler(TagData data, TemplateObject obj);

        /// <summary>
        /// Constructs the TagHelpInfo.
        /// </summary>
        /// <param name="_method">The method to construct from.</param>
        public TagHelpInfo(MethodInfo _method)
        {
            Method = _method;
            Meta = _method.GetCustomAttribute<TagMeta>();
            TEMP_MethodCaller = (MethodHandler) Method.CreateDelegate(typeof(MethodHandler));
        }
    }
}
