//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem;

/// <summary>Represents a command queue when used in an event.</summary>
/// <param name="queue">The relevant queue.</param>
public class CommandQueueEventArgs(CommandQueue queue) : EventArgs
{
    /// <summary>The relevant queue.</summary>
    public CommandQueue Queue = queue;
}
