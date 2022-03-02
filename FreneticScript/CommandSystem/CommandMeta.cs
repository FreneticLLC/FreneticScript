//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript.ScriptSystems;

namespace FreneticScript.CommandSystem
{
    /// <summary>Represents command meta.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandMeta : ScriptMetaAttribute
    {
        /// <summary>The name of the command.</summary>
        public string Name;

        /// <summary>A descriptor string of the arguments to this command.</summary>
        public string Arguments;

        /// <summary>A short description (single line) of this command.</summary>
        public string Short;

        /// <summary>A standard date string (yyyy/mm/dd) of when this command was last updated.</summary>
        public string Updated;

        /// <summary>The minimum number of args this command allows.</summary>
        public int MinimumArgs;

        /// <summary>The maximum number of args this command allows, or -1 if dynamic argument processing.</summary>
        public int MaximumArgs;

        /// <summary>A long full description of this command.</summary>
        public string Description;

        /// <summary>A set of example usages for this command.</summary>
        public string[] Examples;

        /// <summary>Whether the command is for debugging purposes.</summary>
        public bool IsDebug = false;

        /// <summary>Whether the 'break' command can be used on this command.</summary>
        public bool IsBreakable = false;

        /// <summary>Whether the command is part of a script's flow rather than for normal client use.</summary>
        public bool IsFlow = false;

        /// <summary>Whether the command can be &amp;waited on.</summary>
        public bool Waitable = false;

        /// <summary>
        /// Whether the command can be run off the primary tick.
        /// NOTE: These mostly have yet to be confirmed! They are purely theoretical!
        /// </summary>
        public bool Asyncable = false;
    }
}
