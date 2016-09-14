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
