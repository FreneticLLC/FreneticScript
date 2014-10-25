using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.TagHandlers
{
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

        public Variable(string _name, string _value)
        {
            Name = _name;
            Value = _value;
        }
    }
}
