using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem
{
    /// <summary>
    /// The data held by a command.
    /// </summary>
    public abstract class AbstractCommandEntryData
    {
        /// <summary>
        /// Perfectly clone the entrydata object.
        /// </summary>
        public abstract AbstractCommandEntryData Duplicate();
    }
}
