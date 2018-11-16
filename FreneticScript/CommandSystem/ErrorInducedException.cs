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
using System.Runtime.Serialization;
using System.Text;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents an exception induced by a script error. Should be ignored/re-thrown to let the error propogate up to the script level.
    /// </summary>
    [Serializable]
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

    /// <summary>
    /// Represents an exception induced by a script error related to a tag. Should be ignored/re-thrown to let the error propogate up to the script level.
    /// <para>Only for use within the internal script engine systems.</para>
    /// </summary>
    [Serializable]
    public class TagErrorInducedException : ErrorInducedException
    {
        /// <summary>
        /// Constructs a plain tag error induced exception.
        /// </summary>
        public TagErrorInducedException()
        {
        }
        
        /// <summary>
        /// Constructs a tag error induced exception with a message and inner exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public TagErrorInducedException(string message, Exception inner) :
            base(message, inner)
        {
        }

        /// <summary>
        /// Constructs a tag error induced exception with a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public TagErrorInducedException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Serializable system.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization streaming context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(SubTagIndex), SubTagIndex);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Relevant sub-tag index.
        /// </summary>
        public int SubTagIndex;

        /// <summary>
        /// Constructs a tag error induced exception with a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="tagIndex">The relevant sub-tag index.</param>
        public TagErrorInducedException(string message, int tagIndex) :
            base(message)
        {
            SubTagIndex = tagIndex;
        }
    }
}
