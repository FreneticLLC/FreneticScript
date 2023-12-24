//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FreneticScript.CommandSystem;

/// <summary>Represents an exception induced by a script error. Should be ignored/re-thrown to let the error propogate up to the script level.</summary>
[Serializable]
public class ErrorInducedException: Exception
{
    /// <summary>Constructs a plain error induced exception.</summary>
    public ErrorInducedException()
    {
    }

    /// <summary>Constructs an error induced exception with a message and inner exception.</summary>
    /// <param name="message">The message.</param>
    /// <param name="inner">The inner exception.</param>
    public ErrorInducedException(string message, Exception inner) :
        base(message, inner)
    {
    }

    /// <summary>Constructs an error induced exception with a message.</summary>
    /// <param name="message">The message.</param>
    public ErrorInducedException(string message) :
        base(message)
    {
    }

    /// <summary>Microsoft nonsense.</summary>
    protected ErrorInducedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
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
    /// <summary>Constructs a plain tag error induced exception.</summary>
    public TagErrorInducedException()
    {
    }

    /// <summary>Constructs a tag error induced exception with a message and inner exception.</summary>
    /// <param name="message">The message.</param>
    /// <param name="inner">The inner exception.</param>
    public TagErrorInducedException(string message, Exception inner) :
        base(message, inner)
    {
    }

    /// <summary>Constructs a tag error induced exception with a message.</summary>
    /// <param name="message">The message.</param>
    public TagErrorInducedException(string message) :
        base(message)
    {
    }

    /// <summary>Relevant sub-tag index.</summary>
    public int SubTagIndex;

    /// <summary>Constructs a tag error induced exception with a message.</summary>
    /// <param name="message">The message.</param>
    /// <param name="tagIndex">The relevant sub-tag index.</param>
    public TagErrorInducedException(string message, int tagIndex) :
        base(message)
    {
        SubTagIndex = tagIndex;
    }

    /// <summary>Microsoft nonsense.</summary>
    protected TagErrorInducedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
    }
}
