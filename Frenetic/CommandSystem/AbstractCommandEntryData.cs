using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem
{
    public abstract class AbstractCommandEntryData
    {
        public abstract AbstractCommandEntryData Duplicate();
    }
}
