using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }

        /// <summary>
        /// Constructs the argument with input text.
        /// </summary>
        /// <param name="_text">The input text.</param>
        /// <param name="wasquoted">Whether the argument was quoted at input time.</param>
        public TextArgumentBit(string _text, bool wasquoted)
        {
            long ti;
            double tn;
            if (wasquoted)
            {
                InputValue = new TextTag(_text);
            }
            else if (_text == "true")
            {
                InputValue = new BooleanTag(true);
            }
            else if (_text == "false")
            {
                InputValue = new BooleanTag(false);
            }
            else if (_text == "&{NULL}" || _text == "&{null}")
            {
                InputValue = new NullTag();
            }
            else if (long.TryParse(_text, out ti) && ti.ToString() == _text)
            {
                InputValue = new IntegerTag(ti);
            }
            else if (double.TryParse(_text, out tn) && tn.ToString() == _text)
            {
                InputValue = new NumberTag(tn);
            }
            else if (_text.Contains('|'))
            {
                // TODO: Maps stuff?!
                ListTag list = ListTag.For(_text);
                if (list.ToString() == _text)
                {
                    InputValue = list;
                }
                else
                {
                    InputValue = new TextTag(_text);
                }
            }
            else
            {
                InputValue = new TextTag(_text);
            }
        }

        /// <summary>
        /// The input text.
        /// </summary>
        public TemplateObject InputValue;

        /// <summary>
        /// Returns the input text.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The relevant command stack entry, if any.</param>
        /// <returns>The parsed final text.</returns>
        public override TemplateObject Parse(string base_color, DebugMode mode, Action<string> error, CommandStackEntry cse)
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
