//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers;

/// <summary>Represents the specific type of a tag.</summary>
public class TagType
{
    /// <summary>The name of this tag type, lowercase.</summary>
    public string TypeName;

    /// <summary>The name of the type upon which this tag type is based.</summary>
    public string SubTypeName;

    /// <summary>The raw C# / .NET type of the tag type.</summary>
    public Type RawType;

    /// <summary>The type upon which this tag type is based.</summary>
    public TagType SubType;

    /// <summary>The meta for this tag object type.</summary>
    public ObjectMeta Meta;

    /// <summary>The raw internal type (if set to use a raw internal type, otherwise null).</summary>
    public Type RawInternalType;

    /// <summary>The raw internal data field (if set to use a raw internal type, otherwise null).</summary>
    public FieldInfo RawInternalField;

    /// <summary>The raw internal type based object constructor (if set to use a raw internal type, otherwise null).</summary>
    public ConstructorInfo RawInternalConstructor;

    /// <summary>
    /// The method that creates this tag. Set automatically based on raw type.
    /// Has parameters (TemplateObject, TagData) and returns an instance of this tag type.
    /// </summary>
    public MethodInfo CreatorMethod = null;

    /// <summary>
    /// This function should take the two inputs and return a valid object of the relevant type.
    /// TODO: Remove in favor of CreatorMethod!
    /// </summary>
    public Func<TemplateObject, TagData, TemplateObject> TypeGetter;

    /// <summary>The tag sub-handler for all possible tags.</summary>
    public Dictionary<string, TagSubHandler> SubHandlers;

    /// <summary>Contains a mapping of tag names to their helper data. Set automatically based on raw type.</summary>
    public Dictionary<string, TagHelpInfo> TagHelpers;

    /// <summary>
    /// Gets the object of the next type down the tree of types.
    /// REQUIRES a PUBLIC STATIC METHOD be referenced!
    /// TODO: Replace with direct method referencing (to avoid casting trouble).
    /// </summary>
    public Func<TemplateObject, TemplateObject> GetNextTypeDown;

    /// <summary>The tag form of this TagType.</summary>
    public TagTypeTag TagForm;

    /// <summary>The add operation for this type, if any.</summary>
    public ObjectOperationAttribute Operation_Add;

    /// <summary>The subtract operation for this type, if any.</summary>
    public ObjectOperationAttribute Operation_Subtract;

    /// <summary>The multiply operation for this type, if any.</summary>
    public ObjectOperationAttribute Operation_Multiply;

    /// <summary>The divide operation for this type, if any.</summary>
    public ObjectOperationAttribute Operation_Divide;

    /// <summary>The get-sub-settable operation for this type, if any.</summary>
    public ObjectOperationAttribute Operation_GetSubSettable;

    /// <summary>The set operation for this type, if any.</summary>
    public ObjectOperationAttribute Operation_Set;

    /// <summary>An array of operations.</summary>
    public ObjectOperationAttribute[] Operations = new ObjectOperationAttribute[4];

    /// <summary>The backing script engine.</summary>
    public ScriptEngine Engine;

    /// <summary>Builds the <see cref="Operations"/> field.</summary>
    public void BuildOperations()
    {
        Operations[(int)ObjectOperation.ADD] = Operation_Add;
        Operations[(int)ObjectOperation.SUBTRACT] = Operation_Subtract;
        Operations[(int)ObjectOperation.MULTIPLY] = Operation_Multiply;
        Operations[(int)ObjectOperation.DIVIDE] = Operation_Divide;
    }

    /// <summary>Constructs the <see cref="TagType"/>.</summary>
    public TagType()
    {
        TagForm = new TagTypeTag(this);
    }
}
