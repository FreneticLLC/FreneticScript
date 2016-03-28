using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The abstract base for a tag object.
    /// </summary>
    public abstract class TemplateObject
    {
        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public abstract TemplateObject Handle(TagData data);

        /// <summary>
        /// Sets a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to set it to.</param>
        public virtual void Set(string[] names, TemplateObject val)
        {
            throw new Exception("Invalid object setter!");
        }

        /// <summary>
        /// Adds a value to a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to add.</param>
        public virtual void Add(string[] names, TemplateObject val)
        {
            throw new Exception("Invalid object adder!");
        }

        /// <summary>
        /// Subtracts a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to subtract.</param>
        public virtual void Subtract(string[] names, TemplateObject val)
        {
            throw new Exception("Invalid object subtractor!");
        }

        /// <summary>
        /// Multiplies a value by a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to multiply.</param>
        public virtual void Multiply(string[] names, TemplateObject val)
        {
            throw new Exception("Invalid object multiplier!");
        }

        /// <summary>
        /// Divides a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to divide.</param>
        public virtual void Divide(string[] names, TemplateObject val)
        {
            throw new Exception("Invalid object divider!");
        }
    }
}
