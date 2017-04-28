using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles Ternary calculations.
    /// </summary>
    public class TernaryTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base ternary[<BooleanTag>]
        // @Group Text Comparison
        // @ReturnType TernaryPassTag
        // @Returns the specified pass or fail value.
        // The full tag is formatted as <{ternary[<BooleanTag>].pass[<Dynamic>].or_else[<Dynamic>]}>
        // Using <@link tag TernaryPassTag.pass[<Dynamic>]>Pass<@/link> and <@link tag TextTag.or_else[<Dynamic>]>or_else<@/link> sub-tags.
        // If the value in ternary[...] is "true", then the contents of .pass[...] will be returned.
        // Otherwise, the contents of .or_else[...] will be returned.
        // -->

        /// <summary>
        /// Construct the TernaryTags - for internal use only.
        /// </summary>
        public TernaryTagBase()
        {
            Name = "ternary";
            ResultTypeString = "ternarypasstag";
        }

        /// <summary>
        /// Handles the 'ternary[]' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            bool basevalue = (BooleanTag.TryFor(data.GetModifierObject(0)) ?? new BooleanTag(false)).Internal;
            return new TernaryPassTag() { Passed = basevalue };
        }
        
        /// <summary>
        /// Handles Ternary calculations.
        /// </summary>
        public class TernaryPassTag : TemplateObject
        {
            /// <summary>
            /// Whether this ternary tag passed.
            /// </summary>
            public bool Passed = false;

            /// <summary>
            /// Return the type name of this tag.
            /// </summary>
            /// <returns>The tag type name.</returns>
            public override string GetTagTypeName()
            {
                return TYPE;
            }

            /// <summary>
            /// The TernaryPassTag type.
            /// </summary>
            public static string TYPE = "ternarypasstag";

            /// <summary>
            /// Gets a ternary pass tag. Shouldn't be used.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="input">The input.</param>
            public static TernaryPassTag For(TagData data, string input)
            {
                return new TernaryPassTag() { Passed = BooleanTag.For(data, input).Internal };
            }

            /// <summary>
            /// Gets a ternary pass tag. Shouldn't be used.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="input">The input.</param>
            public static TernaryPassTag For(TemplateObject input, TagData data)
            {
                return input is TernaryPassTag ? (TernaryPassTag)input : For(data, input.ToString());
            }
            
            /// <summary>
            /// Returns a boolean true or false as text.
            /// </summary>
            public override string ToString()
            {
                return new BooleanTag(Passed).ToString();
            }
        }
    }
}
