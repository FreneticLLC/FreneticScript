//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers;

/// <summary>Holds a name:value pair.</summary>
/// <param name="_name">The name of the variable.</param>
/// <param name="_value">SeeThe value of the variable.</param>
public class Variable(string _name, string _value)
{
    /// <summary>The name of the variable.</summary>
    public string Name = _name;

    /// <summary>The value of the variable.</summary>
    public string Value = _value;
}
