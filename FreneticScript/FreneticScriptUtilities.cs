using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreneticScript
{
    /// <summary>
    /// Various utilities needed in FreneticScript.
    /// </summary>
    public static class FreneticScriptUtilities
    {
        /// <summary>
        /// Helper to check special handling for an exception, in particular propagating thread abort exceptions.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        public static void CheckException(Exception ex)
        {
            if (ex is ThreadAbortException)
            {
                throw ex;
            }
        }
    }
}
