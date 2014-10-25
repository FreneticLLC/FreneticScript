using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    class UnescapeTags : TemplateTags
    {
        // TODO: META
        public static string Unescape(string input)
        {
            return input.Replace("&rb", "]")
                .Replace("&lb", "[")
                .Replace("&dot", ".")
                .Replace("&sp", " ")
                .Replace("&colon", ":")
                .Replace("&semi", ";")
                .Replace("&pipe", "|")
                .Replace("&amp", "&");
        }

        public UnescapeTags()
        {
            Name = "unescape";
        }

        public override string Handle(TagData data)
        {
            string modif = data.GetModifier(0);
            return new TextTag(Unescape(modif)).Handle(data.Shrink());
        }
    }
}
