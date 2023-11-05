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
using FreneticScript.CommandSystem;

namespace FreneticScript.TagHandlers;

/// <summary>Represents sourcing data for an edit operation on an object, including error calls and any other source marks.</summary>
public class ObjectEditSource
{
    /// <summary>A valid error call, to be used if the set operation fails in a significant way.</summary>
    public Action<string> Error;

    /// <summary>The command entry that started this edit operation (if any).</summary>
    public CommandEntry Entry;

    /// <summary>The command queue that started this edit operation (if any).</summary>
    public CommandQueue Queue;
}
