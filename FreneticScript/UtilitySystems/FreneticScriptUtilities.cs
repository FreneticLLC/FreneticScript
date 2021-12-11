//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FreneticUtilities.FreneticToolkit;
using FreneticScript.CommandSystem;

namespace FreneticScript
{
    /// <summary>Various utilities needed in FreneticScript.</summary>
    public static class FreneticScriptUtilities
    {
        /// <summary>Helper to check special handling for an exception, in particular propagating thread abort exceptions.</summary>
        /// <param name="ex">The exception to check.</param>
        public static void CheckException(Exception ex)
        {
            if (ex is ThreadAbortException)
            {
                throw ex;
            }
        }

        /// <summary>If raw string data is input by a user, call this function to clean it for tag-safety.</summary>
        /// <param name="input">The raw string.</param>
        /// <returns>A cleaned string.</returns>
        public static string CleanStringInput(string input)
        {
            // No nulls!
            return input.Replace('\0', ' ');
        }

        /// <summary>Configures the <see cref="ILGeneratorTracker"/> for FS output handling.</summary>
        public static ILGeneratorTracker ConfigureTracker(this ILGeneratorTracker tracker, ScriptEngine engine)
        {
            tracker.BaseCode = TextStyle.Base;
            tracker.EmphasizeCode = TextStyle.Separate;
            tracker.MinorCode = TextStyle.Minor;
            tracker.Error = engine.Context.BadOutput;
            return tracker;
        }
    }
}
