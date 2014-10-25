using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    class EscapeTags : TemplateTags
    {
        // TODO: META
        public static string Escape(string input)
        {
            return input.Replace("&", "&amp")
                .Replace("|", "&pipe")
                .Replace(";", "&semi")
                .Replace(":", "&colon")
                .Replace(" ", "&sp")
                .Replace(".", "&dot")
                .Replace("[", "&lb")
                .Replace("]", "&rb");
        }

        public EscapeTags()
        {
            Name = "escape";
        }

        public override string Handle(TagData data)
        {
            string modif = data.GetModifier(0);
            return new TextTag(Escape(modif)).Handle(data.Shrink());
        }
    }
}
