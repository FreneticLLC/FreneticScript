//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using FreneticScript.ScriptSystems;

namespace FreneticScript.TagHandlers;

/// <summary>Helps represent a tag.</summary>
/// <param name="_method">The method to construct from.</param>
public class TagHelpInfo(MethodInfo _method)
{
    /// <summary>The tag meta data.</summary>
    public TagMeta Meta = _method.GetCustomAttribute<TagMeta>();

    /// <summary>The relevant method.</summary>
    public MethodInfo Method = _method;

    /// <summary>A helper function to directly run the tag from basic input.</summary>
    public Func<TemplateObject, TagData, TemplateObject> RunTagLive;
}
