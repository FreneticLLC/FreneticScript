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

namespace FreneticScript.TagHandlers
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
