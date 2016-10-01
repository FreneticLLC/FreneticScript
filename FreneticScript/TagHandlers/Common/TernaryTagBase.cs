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
        }

        /// <summary>
        /// Handles the 'ternary[]' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject HandleOne(TagData data)
        {
            bool basevalue = (BooleanTag.TryFor(data.GetModifierObject(0)) ?? new BooleanTag(false)).Internal;
            return new TernaryPassTag() { Passed = basevalue };
        }
        
        /// <summary>
        /// Handles Ternary calculations.
        /// </summary>
        public class TernaryPassTag : TemplateObject
        {
            // TODO: Fully update to new tag system.

            /// <summary>
            /// Whether this ternary tag passed.
            /// </summary>
            public bool Passed = false;

            /// <summary>
            /// Handles the 'ternary[].pass[]' tag.
            /// </summary>
            /// <param name="data">The data to be handled.</param>
            public override TemplateObject Handle(TagData data)
            {
                // <--[tag]
                // @Name TernaryPassTag.pass[<TextTag>]
                // @Group Text Comparison
                // @ReturnType TernaryFailTag
                // @Returns a step in the ternary pass/fail tag.
                // Used as a part of the <@link tag Ternary[<TextTag>]>Ternary<@/link> tag.
                // -->
                if (Passed)
                {
                    return data.GetModifierObject(0).Handle(data.Shrink());
                }
                return new NullTag().Handle(data.Shrink());
            }

            /// <summary>
            /// Returns NullTag.ToString.
            /// </summary>
            public override string ToString()
            {
                return new NullTag().ToString();
            }
        }
    }
}
