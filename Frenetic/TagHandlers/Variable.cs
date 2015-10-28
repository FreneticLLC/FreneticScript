using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers
{
    /// <summary>
    /// Holds a name:value pair.
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the variable.
        /// </summary>
        public string Value;

        /// <summary>
        /// Constructs a variable.
        /// </summary>
        /// <param name="_name">See Variable.Name.</param>
        /// <param name="_value">See Variable.Value.</param>
        public Variable(string _name, string _value)
        {
            Name = _name;
            Value = _value;
        }
    }
}
