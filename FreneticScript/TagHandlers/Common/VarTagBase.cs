using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;

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

        public override TagType Adapt(CompiledCommandStackEntry ccse, Dictionary<string, TagType> types, TagArgumentBit tab, int i, int a)
        {
            TagType returnable = null;
            string vn = tab.Bits[0].Variable.ToString().ToLowerFast();
            for (int x = 0; x < ccse.LocalVarNames.Length; x++)
            {
                if (ccse.LocalVarNames[x] == vn)
                {
                    tab.Start = ccse.Entries[i].Command.CommandSystem.TagSystem.LVar;
                    tab.Bits[0].Key = "\0lvar";
                    tab.Bits[0].Handler = null;
                    tab.Bits[0].OVar = tab.Bits[0].Variable;
                    tab.Bits[0].Variable = new Argument() { WasQuoted = false, Bits = new List<ArgumentBit>() { new TextArgumentBit(x.ToString(), false) } };
                    types.TryGetValue(vn, out returnable);
                    break;
                }
            }
            return returnable;
        }
    }
}
