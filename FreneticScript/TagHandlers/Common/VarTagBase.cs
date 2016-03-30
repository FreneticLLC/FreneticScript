﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

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
        // @ReturnType VariableTag
        // @Returns the specified variable from the queue.
        // <@link explanation Queue Variables>What are queue variables?<@/link>
        // -->
        public VarTagBase()
        {
            Name = "var";
        }

        public override TemplateObject Handle(TagData data)
        {
            string modif = data.GetModifier(0).ToLowerFast();
            if (data.Variables != null)
            {
                TemplateObject value;
                if (data.Variables.TryGetValue(modif, out value))
                {
                    data.Shrink();
                    if (data.Remaining == 0)
                    {
                        return value;
                    }
                    // <--[tag]
                    // @Name VariableTag.exists
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the specified variable exists.
                    // Specifically for the tag <@link tag var[<TextTag>]><{var[<TextTag>]}><@/link>.
                    // -->
                    if (data[0] == "exists")
                    {
                        return new BooleanTag(true).Handle(data.Shrink());
                    }
                    else
                    {
                        return value.Handle(data);
                    }
                }
            }
            data.Shrink();
            if (data.Remaining > 0 && data[0] == "exists")
            {
                return new BooleanTag(false).Handle(data.Shrink());
            }
            else
            {
                data.Error("Invalid variable name!");
                return new NullTag().Handle(data);
            }
        }
    }
}
