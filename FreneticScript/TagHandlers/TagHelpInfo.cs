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
using System.Threading.Tasks;
using System.Runtime;
using System.Reflection;
using FreneticScript.ScriptSystems;

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
        /// A helper function to directly run the tag from basic input.
        /// </summary>
        public Func<TemplateObject, TagData, TemplateObject> RunTagLive;
        
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
