using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;

namespace Frenetic
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
        public Outputter Output;

        /// <summary>
        /// Whether the system has been modified or updated since this variable was last set to false.
        /// This variable is so implementations can save CVars to file only when needed.
        /// </summary>
        public bool Modified = false;

        /// <summary>
        /// System CVars.
        /// </summary>
        public CVar s_osversion, s_user, s_dotnetversion, s_totalram, s_culture, s_processors, s_machinename;

        /// <summary>
        /// Constructs the CVar system.
        /// </summary>
        /// <param name="_output">The outputter to use</param>
        public CVarSystem(Outputter _output)
        {
            CVars = new Dictionary<string, CVar>();
            CVarList = new List<CVar>();
            Output = _output;
            Output.CVarSys = this;

            // System CVars
            s_osversion = Register("s_osversion", Environment.OSVersion.VersionString, CVarFlag.Textual | CVarFlag.ReadOnly, "The name and version of the operating system the engine is being run on.");
            s_user = Register("s_user", Environment.UserName, CVarFlag.Textual | CVarFlag.ReadOnly, "The name of the system user running the engine.");
            s_dotnetversion = Register("s_dotnetversion", Environment.Version.ToString(), CVarFlag.Textual | CVarFlag.ReadOnly, "The system's .NET (CLR) version string.");
#if WINDOWS
            s_totalram = Register("s_totalram", new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory.ToString(), CVarFlag.Numeric | CVarFlag.ReadOnly, "How much RAM the system has.");
#endif
            s_culture = Register("s_culture", System.Globalization.CultureInfo.CurrentUICulture.EnglishName, CVarFlag.Textual | CVarFlag.ReadOnly, "The system culture (locale).");
            s_processors = Register("s_processors", Environment.ProcessorCount.ToString(), CVarFlag.Numeric | CVarFlag.ReadOnly, "The number of processors the system the engine is being run on has.");
            s_machinename = Register("s_machinename", Environment.MachineName, CVarFlag.Textual | CVarFlag.ReadOnly, "The name given to the computer the engine is being run on.");
            // TODO: other system info
        }

        /// <summary>
        /// Registers a new CVar.
        /// </summary>
        /// <param name="CVar">The name of the CVar</param>
        /// <param name="value">The default value</param>
        /// <param name="flags">The flags to set on this CVar</param>
        /// <param name="description">An optional description text for a CVar</param>
        /// <returns>The registered CVar</returns>
        public CVar Register(string CVar, string value, CVarFlag flags, string description = null)
        {
            CVar cvar = new CVar(CVar.ToLower(), value, flags, this);
            cvar.Description = description;
            CVars.Add(CVar, cvar);
            CVarList.Add(cvar);
            return cvar;
        }

        /// <summary>
        /// Sets the value of an existing CVar, or generates a new one.
        /// </summary>
        /// <param name="CVar">The name of the CVar</param>
        /// <param name="value">The value to set it to</param>
        /// <param name="force">Whether to force a server send</param>
        /// <param name="flags_if_new">What flags to set if the CVar is new</param>
        /// <returns>The set CVar</returns>
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
        /// <param name="CVar">The name of the CVar</param>
        /// <param name="value">The default value if it doesn't exist</param>
        /// <returns>The found CVar</returns>
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
        /// <param name="CVar">The name of the CVar</param>
        /// <returns>The found CVar, or null if none</returns>
        public CVar Get(string CVar)
        {
            string cvlow = CVar.ToLower();
            CVar cvar;
            if (CVars.TryGetValue(cvlow, out cvar))
            {
                return cvar;
            }
            return null;
        }
    }
}
