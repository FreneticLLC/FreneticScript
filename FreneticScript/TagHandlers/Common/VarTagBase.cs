using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem;

namespace FreneticScript.TagHandlers.Common
{
    // <--[explanation]
    // @Name Queue Variables
    // @Description
    // Any given <@link explanation queue>queue<@/link> can have defined variables.
    // Variables are defined primarily by the <@link command define>define<@/link> command,
    // but can be added by other tags and commands, such as the <@link command repeat>repeat<@/link> command.
    // To use a queue variable in a tag, simply use the tag <@link tag var[<TextTag>]><{var[<TextTag>]}><@/link>.
    // TODO: Explain better!
    // -->
    class VarTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base var[<TextTag>]
        // @Group Variables
        // @ReturnType <Dynamic>
        // @Returns the specified variable from the queue.
        // <@link explanation Queue Variables>What are queue variables?<@/link>
        // -->
        public VarTagBase()
        {
            Name = "var";
            CanSingle = true;
        }

        public override TemplateObject HandleOne(TagData data)
        {
            string modif = data.GetModifier(0).ToLowerFast();
            if (data.Variables != null)
            {
                ObjectHolder value;
                if (data.Variables.TryGetValue(modif, out value))
                {
                    return value.Internal;
                }
            }
            data.Error("Invalid variable name '" + TagParser.Escape(modif) + "'!");
            return new NullTag();
        }
    }
}
