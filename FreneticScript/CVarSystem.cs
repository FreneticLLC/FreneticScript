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
using FreneticScript.CommandSystem;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript
{
    /// <summary>
    /// A system for handling user controllable data variables.
    /// </summary>
    public class CVarSystem
    {
        /// <summary>
        /// A list of all existent CVars.
        /// </summary>
        public List<CVar> CVarList;

        /// <summary>
        /// A full map of all existent CVars.
        /// </summary>
        public Dictionary<string, CVar> CVars;

        /// <summary>
        /// The client/server outputter to use.
        /// </summary>
        public ScriptEngineContext Output;

        /// <summary>
        /// Whether the system has been modified or updated since this variable was last set to false.
        /// This variable is so implementations can save CVars to file only when needed.
        /// </summary>
        public bool Modified = false;
        
        /// <summary>
        /// Constructs the CVar system.
        /// </summary>
        /// <param name="_output">The outputter to use.</param>
        public CVarSystem(ScriptEngineContext _output)
        {
            CVars = new Dictionary<string, CVar>();
            CVarList = new List<CVar>();
            Output = _output;
            Output.CVarSys = this;
        }

        /// <summary>
        /// Registers a new CVar.
        /// </summary>
        /// <param name="CVar">The name of the CVar.</param>
        /// <param name="value">The default value.</param>
        /// <param name="flags">The flags to set on this CVar.</param>
        /// <param name="description">An optional description text for a CVar.</param>
        /// <returns>The registered CVar.</returns>
        public CVar Register(string CVar, string value, CVarFlag flags, string description = null)
        {
            CVar cvar = new CVar(CVar.ToLowerFast(), value, flags, this)
            {
                Description = description
            };
            CVars.Add(cvar.Name, cvar);
            CVarList.Add(cvar);
            return cvar;
        }

        /// <summary>
        /// Sets the value of an existing CVar, or generates a new one.
        /// </summary>
        /// <param name="CVar">The name of the CVar.</param>
        /// <param name="value">The value to set it to.</param>
        /// <param name="force">Whether to force a server send.</param>
        /// <param name="flags_if_new">What flags to set if the CVar is new.</param>
        /// <returns>The set CVar.</returns>
        public CVar AbsoluteSet(string CVar, string value, bool force = false, CVarFlag flags_if_new = CVarFlag.None)
        {
            CVar gotten = Get(CVar);
            if (gotten == null)
            {
                gotten = Register(CVar, value, CVarFlag.UserMade | flags_if_new);
            }
            else
            {
                gotten.Set(value, force);
            }
            return gotten;
        }

        /// <summary>
        /// Gets an existing CVar, or generates a new one with a specific default value.
        /// </summary>
        /// <param name="CVar">The name of the CVar.</param>
        /// <param name="value">The default value if it doesn't exist.</param>
        /// <returns>The found CVar.</returns>
        public CVar AbsoluteGet(string CVar, string value)
        {
            CVar gotten = Get(CVar);
            if (gotten == null)
            {
                gotten = Register(CVar, value, CVarFlag.UserMade);
            }
            return gotten;
        }

        /// <summary>
        /// Gets the CVar that matches a specified name.
        /// </summary>
        /// <param name="CVar">The name of the CVar.</param>
        /// <returns>The found CVar, or null if none.</returns>
        public CVar Get(string CVar)
        {
            string cvlow = CVar.ToLowerFast();
            if (CVars.TryGetValue(cvlow, out CVar cvar))
            {
                return cvar;
            }
            return null;
        }
    }
}
