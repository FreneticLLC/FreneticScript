//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The master class for parsing tags.
    /// </summary>
    public class TagParser
    {
        // <--[definition]
        // @Word specified
        // @Group tag
        // @Description The word 'specified', when used in a tag description, refers to the value input after the tag.
        // EG, you might have the tag <@link tag TextTag.equals[<TextTag>]><text[<TextTag>].equals[<TextTag>]><@/link>
        // In this tag, the first <TextTag> is referred to as 'the text', and the second <TextTag> as 'specified text'.
        // Which would look like: <text[the text].equals[specified text]>.
        // -->

        /// <summary>
        /// The command system that made this tag system.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// Escapes any tags inside a string.
        /// </summary>
        /// <param name="input">The string that may have tags.</param>
        /// <returns>An escaped string.</returns>
        public static string Escape(string input)
        {
            if (input == null)
            {
                return "null";
            }
            return input.Replace("<", "\0TAGSTART").Replace(">", "\0TAGEND");
        }

        /// <summary>
        /// Reverses any tag escaping inside a string.
        /// </summary>
        /// <param name="input">The string that was escaped.</param>
        /// <returns>An unescaped string that may have tags.</returns>
        public static string Unescape(string input)
        {
            if (input == null)
            {
                return "null";
            }
            return input.Replace("\0TAGSTART", "<").Replace("\0TAGEND", ">");
        }

        /// <summary>
        /// All tag handler objects currently registered.
        /// </summary>
        public Dictionary<string, TemplateTagBase> Handlers = new Dictionary<string, TemplateTagBase>();

        /// <summary>
        /// All tag types currently registered.
        /// </summary>
        public Dictionary<string, TagType> Types = new Dictionary<string, TagType>();

        /// <summary>
        /// Registers a handler object for later usage by tags.
        /// </summary>
        /// <param name="handler">The handler object to register.</param>
        public void Register(TemplateTagBase handler)
        {
            Handlers.Add(handler.Name, handler);
        }

        /// <summary>
        /// Registers a type object for later usage by tags.
        /// </summary>
        /// <param name="type">The type object to register.</param>
        /// <param name="creator">The tag creator method (for SAVABLE data).</param>
        public void Register(TagType type, Func<string, TagData, TemplateObject> creator)
        {
            Types.Add(type.TypeName, type);
            SaveCreators[type.TypeName.ToLowerFastFS()] = creator;
        }

        /// <summary>
        /// Local variable tag base.
        /// </summary>
        public LvarTagBase LVar;

        /// <summary>
        /// Prepares the tag system.
        /// </summary>
        public void Init(Commands _system)
        {
            CommandSystem = _system;
            // Bases
            Register(new BinaryTagBase());
            Register(new BooleanTagBase());
            Register(new CVarTagBase());
            Register(new DynamicTagBase());
            Register(new EscapeTagBase());
            Register(new FromSavedTagBase());
            Register(new IntegerTagBase());
            Register(new ListTagBase());
            Register(LVar = new LvarTagBase());
            Register(new MapTagBase());
            Register(new NullTagBase());
            Register(new NumberTagBase());
            Register(new SaveTagBase());
            Register(new SystemTagBase());
            Register(new TagTypeBase());
            Register(new TernaryTagBase());
            Register(new TextColorTagBase());
            Register(new TextTagBase());
            Register(new TimeTagBase());
            Register(new UnescapeTagBase());
            Register(new VarTagBase());
            // Object types
            Register(Type_Binary = new TagType()
            {
                TypeName = BinaryTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = BinaryTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(BinaryTag)
            }, (inp, dat) => BinaryTag.For(dat, inp));
            Register(Type_Boolean = new TagType()
            {
                TypeName = BooleanTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = BooleanTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(BooleanTag)
            }, (inp, dat) => BooleanTag.For(dat, inp));
            Register(Type_Cvar = new TagType()
            {
                TypeName = CVarTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = CVarTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(CVarTag)
            }, (inp, dat) => CVarTag.For(dat, inp));
            Register(Type_Dynamic = new TagType()
            {
                TypeName = DynamicTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = DynamicTag.CreateFor,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(DynamicTag)
            }, DynamicTag.CreateFromSaved);
            Register(Type_Integer = new TagType()
            {
                TypeName = IntegerTag.TYPE,
                SubTypeName = NumberTag.TYPE,
                TypeGetter = IntegerTag.CreateFor,
                GetNextTypeDown = NumberTag.ForIntegerTag,
                SubHandlers = null,
                RawType = typeof(IntegerTag)
            }, (inp, dat) => IntegerTag.For(dat, inp));
            Register(Type_List = new TagType()
            {
                TypeName = ListTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = ListTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(ListTag)
            }, ListTag.CreateFromSaved);
            Register(Type_Map = new TagType()
            {
                TypeName = MapTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = MapTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(MapTag)
            }, MapTag.CreateFromSaved);
            Register(Type_Null = new TagType()
            {
                TypeName = NullTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = (data, obj) => new NullTag(),
                GetNextTypeDown = TextTag.For, // TODO: Or an error?
                SubHandlers = null,
                RawType = typeof(NullTag)
            }, (inp, dat) => new NullTag());
            Register(Type_Number = new TagType()
            {
                TypeName = NumberTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = NumberTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(NumberTag)
            }, (inp, dat) => NumberTag.For(dat, inp));
            Register(Type_System = new TagType()
            {
                TypeName = SystemTagBase.SystemTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = SystemTagBase.SystemTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(SystemTagBase.SystemTag)
            }, (inp, dat) => SystemTagBase.SystemTag.For(dat, inp));
            Register(Type_TagType = new TagType()
            {
                TypeName = TagTypeTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = TagTypeTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(TagTypeTag)
            }, (inp, dat) => TagTypeTag.For(dat, inp));
            Register(Type_TernayPass = new TagType()
            {
                // TODO: Convert!
                TypeName = "ternarypasstag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = TernaryTagBase.TernaryPassTag.For,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(TernaryTagBase.TernaryPassTag)
            }, (inp, dat) => TernaryTagBase.TernaryPassTag.For(dat, inp));
            Register(Type_Text = new TagType()
            {
                TypeName = TextTag.TYPE,
                SubTypeName = null,
                TypeGetter = TextTag.CreateFor,
                GetNextTypeDown = null,
                SubHandlers = null,
                RawType = typeof(TextTag)
            }, (inp, dat) => new TextTag(inp));
            Register(Type_Time = new TagType()
            {
                TypeName = TimeTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = TimeTag.CreateFor,
                GetNextTypeDown = TextTag.For,
                SubHandlers = null,
                RawType = typeof(TimeTag)
            }, (inp, dat) => TimeTag.For(inp));
        }
        /// <summary>
        /// Set up the tag engine after all input has be registered.
        /// </summary>
        public void PostInit()
        {
            foreach (TemplateTagBase tagbase in Handlers.Values)
            {
                if (tagbase.ResultTypeString != null)
                {
                    tagbase.ResultType = Types[tagbase.ResultTypeString];
                }
            }
            foreach (TagType type in Types.Values)
            {
                if (type.SubTypeName == null)
                {
                    type.SubType = null;
                }
                else
                {
                    type.SubType = Types[type.SubTypeName];
                }
                type.TagHelpers = new Dictionary<string, TagHelpInfo>(500);
                if (type.RawType != null)
                {
                    foreach (MethodInfo method in type.RawType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        TagMeta tm = method.GetCustomAttribute<TagMeta>();
                        if (tm != null)
                        {
                            TagHelpInfo thi = new TagHelpInfo(method);
                            thi.Meta.Ready(this);
                            if (thi.Meta.TagType == DynamicTag.TYPE && thi.Meta.Name == "as")
                            {
                                // Special exception! Specially compiled tag!
                            }
                            else
                            {
                                thi.Meta.ReturnTypeResult = Types[thi.Meta.ReturnType];
                                if (thi.Meta.ReturnTypeResult == null)
                                {
                                    CommandSystem.Output.BadOutput("Bad tag declaration (returns '" + thi.Meta.ReturnType + "'): " + type.TypeName + "." + thi.Meta.Name);
                                }
                            }
                            type.TagHelpers.Add(tm.Name, thi);
                        }
                        else if (method.Name == "CreateFor")
                        {
                            ParameterInfo[] prms = method.GetParameters();
                            if (prms.Length == 2 && prms[0].ParameterType == typeof(TemplateObject) && prms[1].ParameterType == typeof(TagData))
                            {
                                type.CreatorMethod = method;
                            }
                        }
                    }
                    TagHelpInfo auto_thi = new TagHelpInfo(AUTO_OR_ELSE);
                    auto_thi.Meta = auto_thi.Meta.Duplicate();
                    auto_thi.Meta.ReturnTypeResult = Types[auto_thi.Meta.ReturnType];
                    auto_thi.Meta.ActualType = type;
                    type.TagHelpers.Add(auto_thi.Meta.Name, auto_thi);
                }
            }
        }

        /// <summary>
        /// An automatic tag for the 'or_else' system.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        /// <param name="obj">The input object.</param>
        /// <returns>The result as described by meta documentation.</returns>
        [TagMeta(TagType = null, Name = "or_else", Group = "Nulls", ReturnType = DynamicTag.TYPE, Returns = "The current object, or the specified object if the current is null.")]
        public static TemplateObject AutoTag_Or_Else(TemplateObject obj, TagData data)
        {
            // TODO: Special compiler code, to not need a dynamic tag?
            if (obj is NullTag)
            {
                return new DynamicTag(data.GetModifierObject(0));
            }
            return new DynamicTag(obj);
        }

        /// <summary>
        /// References <see cref="AutoTag_Or_Else(TemplateObject, TagData)"/>.
        /// </summary>
        public static MethodInfo AUTO_OR_ELSE = typeof(TagParser).GetMethod("AutoTag_Or_Else");
        
        /// <summary>
        /// The BinaryTag type.
        /// </summary>
        public TagType Type_Binary;

        /// <summary>
        /// The BooleanTag type.
        /// </summary>
        public TagType Type_Boolean;

        /// <summary>
        /// The CVar tag type.
        /// </summary>
        public TagType Type_Cvar;

        /// <summary>
        /// The DynamicTag type.
        /// </summary>
        public TagType Type_Dynamic;

        /// <summary>
        /// The IntegerTag type.
        /// </summary>
        public TagType Type_Integer;

        /// <summary>
        /// The ListTag type.
        /// </summary>
        public TagType Type_List;

        /// <summary>
        /// The MapTag type.
        /// </summary>
        public TagType Type_Map;

        /// <summary>
        /// The NullTag type.
        /// </summary>
        public TagType Type_Null;

        /// <summary>
        /// The NumberTag type.
        /// </summary>
        public TagType Type_Number;

        /// <summary>
        /// The SystemTag type.
        /// </summary>
        public TagType Type_System;

        /// <summary>
        /// The TagTypeTag type.
        /// </summary>
        public TagType Type_TagType;

        /// <summary>
        /// The TernaryPassTag type.
        /// </summary>
        public TagType Type_TernayPass;

        /// <summary>
        /// The TextTag type.
        /// </summary>
        public TagType Type_Text;
        
        /// <summary>
        /// The TimeTag type.
        /// </summary>
        public TagType Type_Time;

        /// <summary>
        /// Helpers to load tags for any given type, input by name.
        /// </summary>
        public Dictionary<string, Func<string, TagData, TemplateObject>> SaveCreators = new Dictionary<string, Func<string, TagData, TemplateObject>>();

        /// <summary>
        /// Creates an object from saved data.
        /// </summary>
        /// <param name="input">The input save data.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>The resultant object.</returns>
        public TemplateObject ParseFromSaved(string input, TagData data)
        {
            string[] dat = input.SplitFastFS(TemplateObject.SAVE_MARK[0], 1);
            if (SaveCreators.TryGetValue(dat[0], out Func<string, TagData, TemplateObject> creator))
            {
                return creator(dat[1], data);
            }
            data.Error("Invalid save loader type (Was a tag type spelled wrong?)!");
            return new NullTag();
        }

        /// <summary>
        /// Splits text into an Argument, for preparsing.
        /// </summary>
        /// <param name="input">The original text.</param>
        /// <param name="wasquoted">Whether the argument was input with "quotes".</param>
        /// <returns>The parsed Argument.</returns>
        public Argument SplitToArgument(string input, bool wasquoted)
        {
            if (input.Length == 0)
            {
                return new Argument() { Bits = new ArgumentBit[0] };
            }
            if (input.IndexOf("<") < 0)
            {
                Argument a = new Argument() { WasQuoted = wasquoted };
                a.Bits = new ArgumentBit[] { new TextArgumentBit(input, wasquoted) { CommandSystem = CommandSystem } };
                a.Compile();
                return a;
            }
            Argument arg = new Argument() { WasQuoted = wasquoted };
            int len = input.Length;
            int blocks = 0;
            int brackets = 0;
            StringBuilder blockbuilder = new StringBuilder();
            StringBuilder tbuilder = new StringBuilder();
            List<ArgumentBit> bitos = new List<ArgumentBit>();
            for (int i = 0; i < len; i++)
            {
                if (input[i] == '<')
                {
                    blocks++;
                    if (blocks == 1)
                    {
                        continue;
                    }
                }
                else if (input[i] == '>')
                {
                    blocks--;
                    if (blocks == 0)
                    {
                        if (tbuilder.Length > 0)
                        {
                            bitos.Add(new TextArgumentBit(tbuilder.ToString(), wasquoted) { CommandSystem = CommandSystem });
                            tbuilder = new StringBuilder();
                        }
                        string value = blockbuilder.ToString();
                        string fallback = null;
                        int brack = 0;
                        for (int fb = 0; fb < value.Length; fb++)
                        {
                            if (value[fb] == '[')
                            {
                                brack++;
                            }
                            if (value[fb] == ']')
                            {
                                brack--;
                            }
                            // TODO: Scrap old fallback engine, in favor of null tricks.
                            if (brack == 0 && value[fb] == '|' && fb > 0 && value[fb - 1] == '|')
                            {
                                fallback = value.Substring(fb + 1);
                                value = value.Substring(0, fb);
                                break;
                            }
                        }
                        string[] split = value.SplitFastFS('.');
                        for (int s = 0; s < split.Length; s++)
                        {
                            split[s] = split[s].Replace("&dot", ".").Replace("&amp", "&");
                        }
                        List<TagBit> bits = new List<TagBit>();
                        for (int x = 0; x < split.Length; x++)
                        {
                            TagBit bit = new TagBit();
                            if (split[x].Length > 1 && split[x].Contains('[') && split[x][split[x].Length - 1] == ']')
                            {
                                int index = split[x].IndexOf('[');
                                bit.Variable = SplitToArgument(split[x].Substring(index + 1, split[x].Length - (index + 2)), wasquoted);
                                split[x] = split[x].Substring(0, index).ToLowerFastFS();
                                if (split[x].Length == 0)
                                {
                                    if (x == 0)
                                    {
                                        split[x] = "var";
                                    }
                                    else
                                    {
                                        split[x] = "get";
                                    }
                                }
                            }
                            else
                            {
                                split[x] = split[x].ToLowerFastFS();
                                bit.Variable = new Argument();
                            }
                            bit.Key = split[x];
                            bits.Add(bit);
                        }
                        TagArgumentBit tab = new TagArgumentBit(CommandSystem, bits.ToArray());
                        if (tab.Bits.Length > 0)
                        {
                            if (!CommandSystem.TagSystem.Handlers.TryGetValue(tab.Bits[0].Key.ToLowerFastFS(), out TemplateTagBase start))
                            {
                                throw new ErrorInducedException("Invalid tag base '" + tab.Bits[0].Key.ToLowerFastFS() + "'!");
                            }
                            tab.Start = start;
                        }
                        tab.Fallback = fallback == null ? null : SplitToArgument(fallback, false);
                        bitos.Add(tab);
                        blockbuilder = new StringBuilder();
                        continue;
                    }
                }
                else if (blocks == 1 && input[i] == '[')
                {
                    brackets++;
                }
                else if (blocks == 1 && input[i] == ']')
                {
                    brackets--;
                }
                if (blocks > 0)
                {
                    switch (input[i])
                    {
                        case '.':
                            if (blocks > 1 || brackets > 0)
                            {
                                blockbuilder.Append("&dot");
                            }
                            else
                            {
                                blockbuilder.Append(".");
                            }
                            break;
                        case '&':
                            blockbuilder.Append("&amp");
                            break;
                        default:
                            blockbuilder.Append(input[i]);
                            break;
                    }
                }
                else
                {
                    tbuilder.Append(input[i]);
                }
            }
            if (tbuilder.Length > 0)
            {
                bitos.Add(new TextArgumentBit(tbuilder.ToString(), wasquoted) { CommandSystem = CommandSystem });
            }
            arg.Bits = bitos.ToArray();
            arg.Compile();
            return arg;
        }
        
        /// <summary>
        /// Reference to <see cref="DebugTagHelper(TemplateObject, TagData)"/>.
        /// </summary>
        public static MethodInfo Method_DebugTagHelper = typeof(TagParser).GetMethod("DebugTagHelper");

        /// <summary>
        /// Helper for debugging compiled tags.
        /// </summary>
        /// <param name="res">The returned object.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>Res, unmodified.</returns>
        public static TemplateObject DebugTagHelper(TemplateObject res, TagData data)
        {
            if (data.mode <= DebugMode.FULL)
            {
                data.TagSystem.CommandSystem.Output.GoodOutput("Filled tag " + TextStyle.Color_Separate +
                    new TagArgumentBit(data.TagSystem.CommandSystem, data.InputKeys).ToString() + TextStyle.Color_Outgood + " with \"" + TextStyle.Color_Separate + res.ToString()
                    + TextStyle.Color_Outgood + "\".");
            }
            return res;
        }
    }
}
