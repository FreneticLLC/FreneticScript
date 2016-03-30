using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a single entry in a command stack.
    /// </summary>
    public class CommandStackEntry
    {
        /// <summary>
        /// The index of the currently running command.
        /// </summary>
        public int Index;

        /// <summary>
        /// All available commands.
        /// </summary>
        public CommandEntry[] Entries;

        /// <summary>
        /// All variables available in this portion of the stack.
        /// </summary>
        public Dictionary<string, TemplateObject> Variables;

        /// <summary>
        /// How much debug information this portion of the stack should show.
        /// </summary>
        public DebugMode Debug;

        /// <summary>
        /// What was returned by the determine command for this portion of the stack.
        /// </summary>
        public List<TemplateObject> Determinations = new List<TemplateObject>();
    }
}
