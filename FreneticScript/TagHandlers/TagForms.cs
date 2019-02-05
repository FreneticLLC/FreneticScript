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
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The form of an <see cref="IntegerTag"/>.
    /// </summary>
    public interface IntegerTagForm
    {
        /// <summary>
        /// The integer value of this <see cref="IntegerTag"/>-like object.
        /// </summary>
        long IntegerForm { get; }
    }

    /// <summary>
    /// The form of a <see cref="NumberTag"/>.
    /// </summary>
    public interface NumberTagForm
    {
        /// <summary>
        /// The number value of this <see cref="NumberTag"/>-like object.
        /// </summary>
        double NumberForm { get; }
    }

    /// <summary>
    /// The form of a <see cref="ListTag"/>.
    /// </summary>
    public interface ListTagForm
    {
        /// <summary>
        /// The <see cref="ListTag"/> value of this <see cref="ListTag"/>-like object.
        /// </summary>
        ListTag ListForm { get; }
    }
}
