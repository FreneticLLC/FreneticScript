//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript
{
    /// <summary>
    /// Command output message types.
    /// </summary>
    public enum MessageType: int
    {
        /// <summary>
        /// No output type, null, 0.
        /// </summary>
        NUL = 0,
        /// <summary>
        /// Bad output type, 1.
        /// </summary>
        BAD = 1,
        /// <summary>
        /// Good output type, 2.
        /// </summary>
        GOOD = 2,
        /// <summary>
        /// Informational output type, 3.
        /// </summary>
        INFO = 3
    }
}
