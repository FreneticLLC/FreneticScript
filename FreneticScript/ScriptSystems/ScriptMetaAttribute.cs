//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticScript.ScriptSystems;

/// <summary>Base class for script meta attribute types.</summary>
public abstract class ScriptMetaAttribute : Attribute
{
    /// <summary>The group this meta documentation piece belongs to.</summary>
    public string Group;

    /// <summary>Any other information/notes for this meta documentation piece.</summary>
    public string[] Others;
}
