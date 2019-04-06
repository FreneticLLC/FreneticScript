//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Emit;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a tag object of unknown type.
    /// </summary>
    [ObjectMeta(Name = DynamicTag.TYPE, SubTypeName = TextTag.TYPE, Group = "Tag System", Description = "Represents any object, dynamically.")]
    public class DynamicTag : TemplateObject
    {

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
            return tagTypeSet.Type_Dynamic;
        }

        // TODO: Explanation of dynamics!

        /// <summary>
        /// The represented tag object.
        /// </summary>
        public TemplateObject Internal;

        /// <summary>
        /// The field <see cref="Internal"/>.
        /// </summary>
        public static readonly FieldInfo Field_DynamicTag_Internal = typeof(DynamicTag).GetField(nameof(Internal));
        
        /// <summary>
        /// Constructs a new DynamicTag.
        /// </summary>
        /// <param name="obj">The TemplateObject to base this DynamicTag off of.</param>
        public DynamicTag(TemplateObject obj)
        {
            Internal = obj;
        }

        /// <summary>
        /// The DynamicTag type.
        /// </summary>
        public const string TYPE = "dynamic";

        /// <summary>
        /// Creates a SystemTag for the given input data.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid time tag.</returns>
        public static DynamicTag CreateFor(TemplateObject input, TagData data)
        {
            return input as DynamicTag ?? new DynamicTag(input);
        }

        /// <summary>
        /// Creates a DynamicTag from the saved object.
        /// Shouldn't realistically ever be called (as dynamic tags will not save as dynamic).
        /// </summary>
        /// <param name="input">The input data.</param>
        /// <param name="data">The tag data.</param>
        /// <returns>The object.</returns>
        public static DynamicTag CreateFromSaved(string input, TagData data)
        {
            return new DynamicTag(data.TagSystem.ParseFromSaved(input, data));
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DynamicTag Tag_Duplicate(DynamicTag obj, TagData data)
        {
            return new DynamicTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (DynamicTag).")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Type(DynamicTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Types.Type_Dynamic);
        }

        [TagMeta(TagType = TYPE, Name = "held_type", Group = "Dynamics", ReturnType = TagTypeTag.TYPE, Returns = "The type of the held object.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagTypeTag Tag_Held_Type(DynamicTag obj, TagData data)
        {
            return new TagTypeTag(obj.Internal.GetTagType(data.TagSystem.Types));
        }

        [TagMeta(TagType = TYPE, Name = "as", SpecialCompiler = true, SpecialTypeHelperName = nameof(TypeHelper_Tag_As)
            , Group = "Dynamics", ReturnType = null, Returns = "The object as the specified type.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagReturnType Compiler_Tag_As(ILGeneratorTracker ilgen, TagArgumentBit tab, int bit, TagReturnType prevType)
        {
            ilgen.Emit(OpCodes.Ldfld, Field_DynamicTag_Internal); // Load field "Internal" on the input DynamicTag instance.
            ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData.
            string type_name = tab.Bits[bit].Variable.ToString();
            TagType varType = tab.Engine.TagSystem.Types.RegisteredTypes[type_name.ToLowerFast()];
            ilgen.Emit(OpCodes.Call, varType.CreatorMethod); // Run the creator method for the type on the input tag.
            return new TagReturnType(varType, false);
        }

        [TagMeta(TagType = TYPE, Name = "_", Group = "Dynamics", ReturnType = TYPE,
            Returns = "The result of whatever tag is given, based on the runtime type of the object, as a DynamicTag.")]
        public static DynamicTag Tag_DynamicAnyProcessor(DynamicTag inputObject, TagData data)
        {
            string tagName = data.Bits[data.cInd].Key;
            TemplateObject tagObject = inputObject.Internal;
            TagType originalType = tagObject.GetTagType(data.TagSystem.Types);
            TagType objectType = originalType;
            TagHelpInfo tagHelper;
            while (!objectType.TagHelpers.TryGetValue(tagName, out tagHelper))
            {
                if (objectType.SubType == null)
                {
                    throw data.Error("Invalid sub-tag '"
                        + TextStyle.Separate + tagName + TextStyle.Base + "' at sub-tag index "
                        + TextStyle.Separate + data.cInd + TextStyle.Base + " for type '"
                        + TextStyle.Separate + originalType.TypeName + TextStyle.Base
                        + (tagName.Trim().Length == 0 ? "' (stray '.' dot symbol?)!" : "' (sub-tag doesn't seem to exist)!"));
                }
                tagObject = objectType.GetNextTypeDown(tagObject);
                objectType = objectType.SubType;
            }
            return new DynamicTag(tagHelper.RunTagLive(tagObject, data));
        }
        
        public static TagReturnType TypeHelper_Tag_As(TagArgumentBit tab, int bit)
        {
            string type_name = tab.Bits[bit].Variable.ToString().ToLowerFast();
            if (tab.Engine.TagSystem.Types.RegisteredTypes.TryGetValue(type_name, out TagType type))
            {
                return new TagReturnType(type);
            }
            throw new ErrorInducedException("Invalid tag type '" + type_name + "' in as[] handler!");
        }

#pragma warning restore 1591

        /// <summary>
        /// Returns savable dynamic tag data.
        /// </summary>
        /// <returns>The data.</returns>
        public override string GetSavableString()
        {
            // Intentionally discard that this tag is dynamic.
            return Internal.GetSavableString();
        }

        /// <summary>
        /// Returns the dynamic tag data.
        /// </summary>
        /// <returns>The data.</returns>
        public override string ToString()
        {
            return Internal.ToString();
        }

        /// <summary>
        /// Gets a "clean" text form of an object for simpler output to debug logs, may have added colors or other details.
        /// </summary>
        /// <returns>The debug-friendly string.</returns>
        public override string GetDebugString()
        {
            return Internal.GetDebugString();
        }

        /// <summary>
        /// Gets the sub-settable object, or null if none.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="name">The sub-settable object name.</param>
        /// <param name="source">The object edit source.</param>
        /// <returns>The sub-settable object, or null.</returns>
        [ObjectOperationAttribute(ObjectOperation.GETSUBSETTABLE, Input = TYPE)]
        public static TemplateObject GetSubSettable(DynamicTag tag, string name, ObjectEditSource source)
        {
            return DebugVarSetCommand.GetSubObject(tag.Internal, source.Entry, source.Queue, name);
        }

        /// <summary>
        /// Sets a sub-object by name.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value to insert.</param>
        /// <param name="name">The sub-object name to insert.</param>
        /// <param name="source">The object edit source.</param>
        [ObjectOperationAttribute(ObjectOperation.SET, Input = TYPE)]
        public static void SetSubObject(DynamicTag tag, TemplateObject value, string name, ObjectEditSource source)
        {
            DebugVarSetCommand.SetSubObject(tag.Internal, value, source.Entry, source.Queue, name);
        }

        /// <summary>
        /// Adds a value to the object.
        /// </summary>
        /// <param name="first">The value on the left side of the operator.</param>
        /// <param name="val">The value to add.</param>
        /// <param name="source">The object edit source.</param>
        [ObjectOperationAttribute(ObjectOperation.ADD, Input = TYPE)]
        public static DynamicTag Add(DynamicTag first, DynamicTag val, ObjectEditSource source)
        {
            return new DynamicTag(DebugVarSetCommand.Operate(first.Internal, val.Internal, source.Entry, source.Queue, ObjectOperation.ADD));
        }

        /// <summary>
        /// Subtracts a value from the object.
        /// </summary>
        /// <param name="first">The value on the left side of the operator.</param>
        /// <param name="val">The value to subtract.</param>
        /// <param name="source">The object edit source.</param>
        [ObjectOperationAttribute(ObjectOperation.SUBTRACT, Input = TYPE)]
        public static DynamicTag Subtract(DynamicTag first, DynamicTag val, ObjectEditSource source)
        {
            return new DynamicTag(DebugVarSetCommand.Operate(first.Internal, val.Internal, source.Entry, source.Queue, ObjectOperation.SUBTRACT));
        }

        /// <summary>
        /// Multiplies the object by a value.
        /// </summary>
        /// <param name="first">The value on the left side of the operator.</param>
        /// <param name="val">The value to multiply by.</param>
        /// <param name="source">The object edit source.</param>
        [ObjectOperationAttribute(ObjectOperation.MULTIPLY, Input = TYPE)]
        public static DynamicTag Multiply(DynamicTag first, DynamicTag val, ObjectEditSource source)
        {
            return new DynamicTag(DebugVarSetCommand.Operate(first.Internal, val.Internal, source.Entry, source.Queue, ObjectOperation.MULTIPLY));
        }

        /// <summary>
        /// Divides the object by a value.
        /// </summary>
        /// <param name="first">The value on the left side of the operator.</param>
        /// <param name="val">The value to divide by.</param>
        /// <param name="source">The object edit source.</param>
        [ObjectOperationAttribute(ObjectOperation.DIVIDE, Input = TYPE)]
        public static DynamicTag Divide(DynamicTag first, DynamicTag val, ObjectEditSource source)
        {
            return new DynamicTag(DebugVarSetCommand.Operate(first.Internal, val.Internal, source.Entry, source.Queue, ObjectOperation.DIVIDE));
        }
    }
}
