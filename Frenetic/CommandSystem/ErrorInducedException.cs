using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem
{
    /// <summary>
    /// Represents an exception induced by a script error. Should be ignored/re-thrown to let the error propogate up to the script level.
    /// </summary>
    public class ErrorInducedException: Exception
    {
    }
}
