//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.ScriptSystems;
using System.Reflection;
using System.Reflection.Emit;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The master class for parsing tags.
    /// </summary>
    public class TagHandler
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
        /// The command engine that holds this tag handler.
        /// </summary>
        public ScriptEngine Engine;

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
        /// The tag types handler.
        /// </summary>
        public TagTypes Types = new TagTypes();

        /// <summary>
        /// Registers a handler object for later usage by tags.
        /// </summary>
        /// <param name="handler">The handler object to register.</param>
        public void Register(TemplateTagBase handler)
        {
            Handlers.Add(handler.Name, handler);
        }

        /// <summary>
        /// Local variable tag base.
        /// </summary>
        public LvarTagBase LVar;

        /// <summary>
        /// Prepares the tag system.
        /// </summary>
        public void Init(ScriptEngine _system)
        {
            Engine = _system;
            // Common Object Bases
            Register(new BinaryTagBase());
            Register(new BooleanTagBase());
            Register(new CVarTagBase());
            Register(new DynamicTagBase());
            Register(new FunctionTagBase());
            Register(new IntegerTagBase());
            Register(new ListTagBase());
            Register(new MapTagBase());
            Register(new NullTagBase());
            Register(new NumberTagBase());
            Register(new TagTypeBase());
            Register(new TextTagBase());
            Register(new TimeTagBase());
            // Helper Bases
            Register(new EscapeTagBase());
            Register(new FromSavedTagBase());
            Register(LVar = new LvarTagBase());
            Register(new SaveTagBase());
            Register(new SystemTagBase());
            Register(new TernaryTagBase());
            Register(new TextColorTagBase());
            Register(new UnescapeTagBase());
            Register(new VarTagBase());
            // Object types
            Types.RegisterDefaultTypes();
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
                    if (!Types.RegisteredTypes.TryGetValue(tagbase.ResultTypeString, out tagbase.ResultType))
                    {
                        Engine.Context.BadOutput("TagBase " + tagbase.Name + " (" + tagbase.GetType().FullName + ") failed to parse: invalid result type '" + tagbase.ResultTypeString + "'.");
                    }
                }
            }
            foreach (TagType type in Types.RegisteredTypes.Values)
            {
                if (type.SubTypeName == null)
                {
                    type.SubType = null;
                }
                else
                {
                    type.SubType = Types.RegisteredTypes[type.SubTypeName];
                }
                type.TagHelpers = new Dictionary<string, TagHelpInfo>(500);
                if (type.RawType == null)
                {
                    Engine.Context.BadOutput("Possible bad tag declaration (no RawType): " + type.TypeName);
                }
                else
                {
                    foreach (MethodInfo method in type.RawType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        TagMeta tm = method.GetCustomAttribute<TagMeta>();
                        if (tm != null)
                        {
                            TagHelpInfo thi = new TagHelpInfo(method);
                            thi.Meta.Ready(this);
                            if (thi.Meta.SpecialCompiler)
                            {
                                thi.Meta.SpecialCompileAction = method.CreateDelegate(typeof(Func<CILAdaptationValues.ILGeneratorTracker, TagArgumentBit, int, TagType, TagType>))
                                    as Func<CILAdaptationValues.ILGeneratorTracker, TagArgumentBit, int, TagType, TagType>;
                            }
                            else if (thi.Meta.ReturnTypeResult == null)
                            {
                                Engine.Context.BadOutput("Bad tag declaration (returns '" + thi.Meta.ReturnType + "'): " + type.TypeName + "." + thi.Meta.Name);
                            }
                            if (thi.Meta.SpecialTypeHelperName != null)
                            {
                                thi.Meta.SpecialTypeHelper = type.RawType.GetMethod(thi.Meta.SpecialTypeHelperName, BindingFlags.Static | BindingFlags.Public)
                                    .CreateDelegate(typeof(Func<TagArgumentBit, int, TagType>)) as Func<TagArgumentBit, int, TagType>;
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
                        ObjectOperationAttribute operation = method.GetCustomAttribute<ObjectOperationAttribute>();
                        if (operation != null)
                        {
                            operation.Method = method;
                            operation.GenerateFunctions();
                            switch (operation.Operation)
                            {
                                case ObjectOperation.ADD:
                                    type.Operation_Add = operation;
                                    break;
                                case ObjectOperation.SUBTRACT:
                                    type.Operation_Subtract = operation;
                                    break;
                                case ObjectOperation.MULTIPLY:
                                    type.Operation_Multiply = operation;
                                    break;
                                case ObjectOperation.DIVIDE:
                                    type.Operation_Divide = operation;
                                    break;
                                case ObjectOperation.SET:
                                    type.Operation_Set = operation;
                                    break;
                                case ObjectOperation.GETSUBSETTABLE:
                                    type.Operation_GetSubSettable = operation;
                                    break;
                            }
                        }
                    }
                    type.BuildOperations();
                    TagHelpInfo auto_thi = new TagHelpInfo(AUTO_OR_ELSE);
                    auto_thi.Meta = auto_thi.Meta.Duplicate();
                    auto_thi.Meta.ReturnTypeResult = Types.RegisteredTypes[auto_thi.Meta.ReturnType];
                    auto_thi.Meta.ActualType = type;
                    type.TagHelpers.Add(auto_thi.Meta.Name, auto_thi);
                    if (type.CreatorMethod == null)
                    {
                        Engine.Context.BadOutput("Possible bad tag declaration (no CreateFor method): " + type.TypeName);
                    }
                }
            }
            foreach (TagType type in Types.RegisteredTypes.Values)
            {
                foreach (TagHelpInfo helper in type.TagHelpers.Values)
                {
                    helper.RunTagLive = ScriptCompiler.GenerateTagMethodCallable(helper.Method, helper.Meta, Engine);
                }
            }
        }

        private static readonly Type[] SET_METHOD_PARAMETERS = new Type[] { typeof(string[]), typeof(TemplateObject), typeof(ObjectEditSource) };

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
                return new DynamicTag(data.GetModifierObjectCurrent());
            }
            return new DynamicTag(obj);
        }

        /// <summary>
        /// References <see cref="AutoTag_Or_Else(TemplateObject, TagData)"/>.
        /// </summary>
        public static MethodInfo AUTO_OR_ELSE = typeof(TagHandler).GetMethod(nameof(AutoTag_Or_Else));
        
        /// <summary>
        /// Creates an object from saved data.
        /// </summary>
        /// <param name="input">The input save data.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>The resultant object.</returns>
        public TemplateObject ParseFromSaved(string input, TagData data)
        {
            string[] dat = input.SplitFast(TemplateObject.SAVE_MARK[0], 1);
            if (Types.SaveCreators.TryGetValue(dat[0], out Func<string, TagData, TemplateObject> creator))
            {
                return creator(dat[1], data);
            }
            data.Error("Invalid save loader type (Was a tag type spelled wrong?)!");
            return NullTag.NULL_VALUE;
        }
        
        /// <summary>
        /// Reference to <see cref="DebugTagHelper(TemplateObject, TagData)"/>.
        /// </summary>
        public static MethodInfo Method_DebugTagHelper = typeof(TagHandler).GetMethod(nameof(DebugTagHelper));

        /// <summary>
        /// Helper for debugging compiled tags.
        /// </summary>
        /// <param name="resultObject">The returned object.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>Res, unmodified.</returns>
        public static TemplateObject DebugTagHelper(TemplateObject resultObject, TagData data)
        {
            if (data.DBMode <= DebugMode.FULL)
            {
                string outputText = "Filled tag " + TextStyle.Separate +
                    new TagArgumentBit(data.TagSystem.Engine, data.Bits).ToString() + TextStyle.Outgood + " with \"" + TextStyle.Separate + resultObject.GetDebugString()
                    + TextStyle.Outgood + "\".";
                if (data.CSE != null)
                {
                    if (data.CSE.CurrentCommandEntry != null && data.CSE.CurrentQueue != null)
                    {
                        data.CSE.CurrentCommandEntry.GoodOutput(data.CSE.CurrentQueue, outputText);
                        return resultObject;
                    }
                }
                data.TagSystem.Engine.Context.GoodOutput(outputText);
            }
            return resultObject;
        }
    }
}
