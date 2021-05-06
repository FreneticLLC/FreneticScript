//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FreneticScript.ScriptSystems;
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
        /// Constructs the argument with an input object.
        /// </summary>
        /// <param name="_object">The input object.</param>
        /// <param name="_engine">The backing script engine.</param>
        public TextArgumentBit(TemplateObject _object, ScriptEngine _engine)
        {
            InputValue = _object;
            ResType = _object.GetTagTypeName();
            Engine = _engine;
        }

        /// <summary>
        /// Constructs the argument with an input boolean.
        /// </summary>
        /// <param name="_text">The input boolean.</param>
        /// <param name="_engine">The backing script engine.</param>
        public TextArgumentBit(bool _text, ScriptEngine _engine)
        {
            InputValue = BooleanTag.ForBool(_text);
            ResType = BooleanTag.TYPE;
            Engine = _engine;
        }

        /// <summary>
        /// Constructs the argument with an input integer.
        /// </summary>
        /// <param name="_text">The input integer.</param>
        /// <param name="_engine">The backing script engine.</param>
        public TextArgumentBit(long _text, ScriptEngine _engine)
        {
            InputValue = new IntegerTag(_text);
            ResType = IntegerTag.TYPE;
            Engine = _engine;
        }

        /// <summary>
        /// Constructs the argument with input text.
        /// </summary>
        /// <param name="_text">The input text.</param>
        /// <param name="wasquoted">Whether the argument was quoted at input time.</param>
        /// <param name="perfect">Whether the argument must parse back "perfectly" (meaning, it will ToString to the exact original input).</param>
        /// <param name="_engine">The backing script engine.</param>
        public TextArgumentBit(string _text, bool wasquoted, bool perfect, ScriptEngine _engine)
        {
            Engine = _engine;
            if (wasquoted)
            {
                InputValue = new TextTag(_text);
                ResType = TextTag.TYPE;
                return;
            }
            else if (_text == "true")
            {
                InputValue = BooleanTag.TRUE;
                ResType = BooleanTag.TYPE;
                return;
            }
            else if (_text == "false")
            {
                InputValue = BooleanTag.FALSE;
                ResType = BooleanTag.TYPE;
                return;
            }
            else if (_text == "&{NULL}")
            {
                InputValue = NullTag.NULL_VALUE;
                ResType = NullTag.TYPE;
                return;
            }
            else if (long.TryParse(_text, out long ti) && ti.ToString() == _text)
            {
                InputValue = new IntegerTag(ti);
                ResType = IntegerTag.TYPE;
                return;
            }
            else if (double.TryParse(_text, out double tn) && (!perfect || tn.ToString() == _text))
            {
                InputValue = new NumberTag(tn);
                ResType = NumberTag.TYPE;
                return;
            }
            else if (_text.Contains('|'))
            {
                if (_text.Contains(':'))
                {
                    MapTag map = MapTag.For(_text);
                    if (map.ToString() == _text)
                    {
                        InputValue = map;
                        ResType = MapTag.TYPE;
                        return;
                    }
                }
                ListTag list = ListTag.For(_text);
                if (list.ToString() == _text)
                {
                    InputValue = list;
                    ResType = ListTag.TYPE;
                    return;
                }
            }
            InputValue = new TextTag(_text);
            ResType = TextTag.TYPE;
        }

        /// <summary>
        /// Gets the resultant type of this argument bit.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public override TagReturnType ReturnType(CILAdaptationValues values)
        {
            return new TagReturnType(Engine.TagSystem.Types.RegisteredTypes[ResType], false);
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
        /// <param name="runnable">The command runnable.</param>
        /// <returns>The parsed final text.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override TemplateObject Parse(Action<string> error, CompiledCommandRunnable runnable)
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
