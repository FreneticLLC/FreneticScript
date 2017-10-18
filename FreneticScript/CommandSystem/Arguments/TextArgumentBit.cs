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
using System.Runtime.CompilerServices;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// An argument part containing only plain text.
    /// </summary>
    public class TextArgumentBit : ArgumentBit
    {
        /// <summary>
        /// Constructs the argument with an input boolean.
        /// </summary>
        /// <param name="_text">The input boolean.</param>
        public TextArgumentBit(bool _text)
        {
            InputValue = new BooleanTag(_text);
            ResType = BooleanTag.TYPE;
        }

        /// <summary>
        /// Gets the resultant type of this argument bit.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public override TagType ReturnType(CILAdaptationValues values)
        {
            return CommandSystem.TagSystem.Types[ResType];
        }

        /// <summary>
        /// Constructs the argument with an input integer.
        /// </summary>
        /// <param name="_text">The input integer.</param>
        public TextArgumentBit(long _text)
        {
            InputValue = new IntegerTag(_text);
        }

        /// <summary>
        /// Constructs the argument with input text.
        /// </summary>
        /// <param name="_text">The input text.</param>
        /// <param name="wasquoted">Whether the argument was quoted at input time.</param>
        public TextArgumentBit(string _text, bool wasquoted)
        {
            if (wasquoted)
            {
                InputValue = new TextTag(_text);
                ResType = TextTag.TYPE;
            }
            else if (_text == "true")
            {
                InputValue = new BooleanTag(true);
                ResType = BooleanTag.TYPE;
            }
            else if (_text == "false")
            {
                InputValue = new BooleanTag(false);
                ResType = BooleanTag.TYPE;
            }
            else if (_text == "&{NULL}")
            {
                InputValue = new NullTag();
                ResType = NullTag.TYPE;
            }
            else if (long.TryParse(_text, out long ti) && ti.ToString() == _text)
            {
                InputValue = new IntegerTag(ti);
                ResType = IntegerTag.TYPE;
            }
            else if (double.TryParse(_text, out double tn) && tn.ToString() == _text)
            {
                InputValue = new NumberTag(tn);
                ResType = NumberTag.TYPE;
            }
            else if (_text.Contains('|'))
            {
                // TODO: Maps stuff?!
                ListTag list = ListTag.For(_text);
                if (list.ToString() == _text)
                {
                    InputValue = list;
                    ResType = ListTag.TYPE;
                }
                else
                {
                    InputValue = new TextTag(_text);
                    ResType = TextTag.TYPE;
                }
            }
            else
            {
                InputValue = new TextTag(_text);
                ResType = TextTag.TYPE;
            }
        }

        /// <summary>
        /// The type resultant of this text argument bit.
        /// </summary>
        public string ResType;

        /// <summary>
        /// The input text.
        /// </summary>
        public TemplateObject InputValue;

        /// <summary>
        /// Returns the input text.
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        /// <returns>The parsed final text.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override TemplateObject Parse(Action<string> error, CompiledCommandStackEntry cse)
        {
            return InputValue;
        }

        /// <summary>
        /// Returns the input text.
        /// </summary>
        /// <returns>The input text.</returns>
        public override string ToString()
        {
            return InputValue.ToString();
        }
    }
}
