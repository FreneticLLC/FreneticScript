//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The abstract base for a tag object.
    /// </summary>
    public abstract class TemplateObject
    {
        /// <summary>
        /// Returns the input as-is, for use with Object Types.
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
        /// Return the type of this tag.
        /// </summary>
        /// <returns>The tag type.</returns>
        public virtual TagType GetTagType(TagTypes tagTypeSet)
        {
            return tagTypeSet.TypeForName(GetTagTypeName());
        }

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
        /// Gets a "clean" text form of an object for simpler output to debug logs, may have added colors or other details.
        /// </summary>
        /// <returns>The debug-friendly string.</returns>
        public virtual string GetDebugString()
        {
            return ToString();
        }

        /// <summary>
        /// A generic edit source, where errors are treated as exceptions.
        /// </summary>
        public static readonly ObjectEditSource GENERIC_EDIT_SOURCE = new ObjectEditSource() { Error = (s) => throw new ErrorInducedException("Error in edit operation: " + s) };

        // TODO: Static all the edit methods, for compiler magic reasons?

        /// <summary>
        /// Sets a value on the object, fast. This generally fully overrides the value of an object (as it does not have a sub-key name).
        /// This is expected to operate as a quick brute set. There is no sourcing data and failures may result in system level exceptions. ErrorInducedExceptions should be handled more cleanly.
        /// This is used purely for efficiency with simpler sets. Implementations should only be made if they can run faster or better than a normal Set call.
        /// </summary>
        /// <param name="val">The value to set it to.</param>
        public virtual void SetFast(TemplateObject val)
        {
            Set(null, val, GENERIC_EDIT_SOURCE);
        }

        /// <summary>
        /// Sets a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to set it to.</param>
        /// <param name="src">Source data.</param>
        public virtual void Set(string[] names, TemplateObject val, ObjectEditSource src)
        {
            if (names == null || names.Length == 0)
            {
                src.Error("Invalid object setter for object '" + GetSavableString() + "' (direct set).");
            }
            else
            {
                src.Error("Invalid object setter for object '" + GetSavableString() + "', had sub-name(s) expected: '" + string.Join(".", names) + "'");
            }
        }
        
        /// <summary>
        /// Adds a value to a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to add.</param>
        /// <param name="src">Source data.</param>
        public virtual void Add(string[] names, TemplateObject val, ObjectEditSource src)
        {
            if (names == null || names.Length == 0)
            {
                src.Error("Invalid object adder for object '" + GetSavableString() + "' (direct add).");
            }
            else
            {
                src.Error("Invalid object adder for object '" + GetSavableString() + "', had sub-name(s) expected: '" + string.Join(".", names) + "'");
            }
        }

        /// <summary>
        /// Subtracts a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to subtract.</param>
        /// <param name="src">Source data.</param>
        public virtual void Subtract(string[] names, TemplateObject val, ObjectEditSource src)
        {
            if (names == null || names.Length == 0)
            {
                src.Error("Invalid object subtractor for object '" + GetSavableString() + "' (direct subtract).");
            }
            else
            {
                src.Error("Invalid object subtractor for object '" + GetSavableString() + "', had sub-name(s) expected: '" + string.Join(".", names) + "'");
            }
        }

        /// <summary>
        /// Multiplies a value by a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to multiply.</param>
        /// <param name="src">Source data.</param>
        public virtual void Multiply(string[] names, TemplateObject val, ObjectEditSource src)
        {
            if (names == null || names.Length == 0)
            {
                src.Error("Invalid object multiplier for object '" + GetSavableString() + "' (direct multiply).");
            }
            else
            {
                src.Error("Invalid object multiplier for object '" + GetSavableString() + "', had sub-name(s) expected: '" + string.Join(".", names) + "'");
            }
        }

        /// <summary>
        /// Divides a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to divide.</param>
        /// <param name="src">Source data.</param>
        public virtual void Divide(string[] names, TemplateObject val, ObjectEditSource src)
        {
            if (names == null || names.Length == 0)
            {
                src.Error("Invalid object divider for object '" + GetSavableString() + "' (direct divide).");
            }
            else
            {
                src.Error("Invalid object divider for object '" + GetSavableString() + "', had sub-name(s) expected: '" + string.Join(".", names) + "'");
            }
        }
    }
}
