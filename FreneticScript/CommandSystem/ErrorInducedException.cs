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

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents an exception induced by a script error. Should be ignored/re-thrown to let the error propogate up to the script level.
    /// </summary>
    public class ErrorInducedException: Exception
    {
        /// <summary>
        /// Constructs a plain error induced exception.
        /// </summary>
        public ErrorInducedException()
        {

        }


        /// <summary>
        /// Constructs an error induced exception with a message and inner exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public ErrorInducedException(string message, Exception inner) :
            base(message, inner)
        {
        }

        /// <summary>
        /// Constructs an error induced exception with a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public ErrorInducedException(string message) :
            base(message)
        {
        }
    }
}
