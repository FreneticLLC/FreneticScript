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

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The form of an integer tag.
    /// </summary>
    public interface IntegerTagForm
    {
        /// <summary>
        /// The integer value of this IntegerTag-like object.
        /// </summary>
        long IntegerForm { get; }
    }

    /// <summary>
    /// The form of a number tag.
    /// </summary>
    public interface NumberTagForm
    {
        /// <summary>
        /// The number value of this NumberTag-like object.
        /// </summary>
        double NumberForm { get; }
    }
}
