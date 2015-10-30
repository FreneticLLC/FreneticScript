using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.Arguments
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
        }

        /// <summary>
        /// The input text.
        /// </summary>
        public string Text = null;

        /// <summary>
        /// Returns the input text.
        /// </summary>
        /// <param name="base_color">The base color for color tags.</param>
        /// <param name="vars">The variables for var tags.</param>
        /// <param name="mode">The debug mode to use when parsing tags.</param>
        /// <returns>The parsed final text.</returns>
        public override string Parse(string base_color, Dictionary<string, TemplateObject> vars, DebugMode mode)
        {
            return Text;
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
