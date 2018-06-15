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
