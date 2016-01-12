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
        /// Constructs the argument with input text.
        /// </summary>
        /// <param name="_text">The input text.</param>
        public TextArgumentBit(string _text)
        {
            Text = _text;
            if (Text == "true")
            {
                BoolTag = new BooleanTag(true);
            }
            if (Text == "false")
            {
                BoolTag = new BooleanTag(true);
            }
            double tn;
            if (double.TryParse(Text, out tn) && tn.ToString() == Text)
            {
                NumTag = new NumberTag(tn);
            }
        }

        /// <summary>
        /// The input text.
        /// </summary>
        public string Text = null;

        BooleanTag BoolTag = null;

        NumberTag NumTag = null;

        /// <summary>
        /// Returns the input text.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="vars">The variables for var tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <returns>The parsed final text.</returns>
        public override TemplateObject Parse(string base_color, Dictionary<string, TemplateObject> vars, DebugMode mode, Action<string> error)
        {
            if (NumTag != null)
            {
                return NumTag;
            }
            if (BoolTag != null)
            {
                return BoolTag;
            }
            return new TextTag(Text);
        }

        /// <summary>
        /// Returns the input text.
        /// </summary>
        /// <returns>The input text.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
