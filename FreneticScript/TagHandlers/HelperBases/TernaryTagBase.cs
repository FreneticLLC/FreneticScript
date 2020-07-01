//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.HelperBases
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
            ResultTypeString = TernaryPassTag.TYPE;
        }

        // TODO: Special compiler to handle type continuation (the "or_else[]" should be treated as returning the type input to both "pass[]" and "or_else[]")

        /// <summary>
        /// Handles the 'ternary[]' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TernaryPassTag HandleOne(TagData data)
        {
            bool basevalue = (BooleanTag.TryFor(data.GetModifierObjectCurrent()) ?? BooleanTag.FALSE).Internal;
            return new TernaryPassTag() { Passed = basevalue };
        }
        
        /// <summary>
        /// Handles Ternary calculations.
        /// </summary>
        [ObjectMeta(Name = TernaryPassTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Utilities", Description = "Partial component for ternary tags.")]
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
            /// Return the type of this tag.
            /// </summary>
            /// <returns>The tag type.</returns>
            public override TagType GetTagType(TagTypes tagTypeSet)
            {
                return tagTypeSet.Type_TernayPass;
            }

            /// <summary>
            /// The TernaryPassTag type.
            /// </summary>
            public const string TYPE = "ternarypasstagbase";

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
                return input as TernaryPassTag ?? For(data, input.ToString());
            }

            /// <summary>
            /// Creates a TernaryPassTag for the given input data.
            /// </summary>
            /// <param name="dat">The tag data.</param>
            /// <param name="input">The text input.</param>
            /// <returns>A valid TernaryPassTag.</returns>
            public static TernaryPassTag CreateFor(TemplateObject input, TagData dat)
            {
                return input switch
                {
                    TernaryPassTag tptag => tptag,
                    DynamicTag dtag => CreateFor(dtag.Internal, dat),
                    _ => For(dat, input.ToString()),
                };
            }

            /// <summary>
            /// Returns a boolean true or false as text.
            /// </summary>
            public override string ToString()
            {
                return BooleanTag.ForBool(Passed).ToString();
            }
        }
    }
}
