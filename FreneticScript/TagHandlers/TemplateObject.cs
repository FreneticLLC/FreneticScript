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
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The abstract base for a tag object.
    /// </summary>
    public abstract class TemplateObject
    {
        /// <summary>
        /// Returns the input as-is, for use with ObjecTypes.
        /// </summary>
        /// <param name="obj">The object input.</param>
        /// <returns>The object input.</returns>
        public static TemplateObject Basic_For(TemplateObject obj)
        {
            return obj;
        }

        /// <summary>
        /// Return the type name of this tag.
        /// </summary>
        /// <returns>The tag type name.</returns>
        public abstract string GetTagTypeName();

        /// <summary>
        /// The symbol that connects a type to it's savable data.
        /// </summary>
        public const string SAVE_MARK = "@";

        /// <summary>
        /// Gets the savable string for this instance, including any relevant type information.
        /// </summary>
        /// <returns>The save string.</returns>
        public virtual string GetSavableString()
        {
            return GetTagTypeName() + SAVE_MARK + ToString();
        }

        /// <summary>
        /// Sets a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to set it to.</param>
        public virtual void Set(string[] names, TemplateObject val)
        {
            throw new Exception("Invalid object setter!");
        }

        // TODO: replace virtuals below with automatic tag magic!

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
