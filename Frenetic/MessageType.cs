using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic
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
