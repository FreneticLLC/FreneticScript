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
