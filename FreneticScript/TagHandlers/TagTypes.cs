//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers.HelperBases;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers;

/// <summary>Helper class for tag types recognized within a script system.</summary>
public class TagTypes
{
    /// <summary>All tag types currently registered.</summary>
    public Dictionary<string, TagType> RegisteredTypes = [];

    /// <summary>Gets the type for a type name. Returns null if not found.</summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The type, or null.</returns>
    public TagType TypeForName(string name)
    {
        if (RegisteredTypes.TryGetValue(name, out TagType found))
        {
            return found;
        }
        return null;
    }

    /// <summary>The backing script engine.</summary>
    public ScriptEngine Engine;

    /// <summary>Helpers to load tags for any given type, input by name.</summary>
    public Dictionary<string, Func<string, TagData, TemplateObject>> SaveCreators = [];

    /// <summary>The Binary type.</summary>
    public TagType Type_Binary;

    /// <summary>The Boolean type.</summary>
    public TagType Type_Boolean;

    /// <summary>The Dynamic type.</summary>
    public TagType Type_Dynamic;

    /// <summary>The Function type.</summary>
    public TagType Type_Function;

    /// <summary>The Integer type.</summary>
    public TagType Type_Integer;

    /// <summary>The List type.</summary>
    public TagType Type_List;

    /// <summary>The Map type.</summary>
    public TagType Type_Map;

    /// <summary>The Null type.</summary>
    public TagType Type_Null;

    /// <summary>The Number type.</summary>
    public TagType Type_Number;

    /// <summary>The System type.</summary>
    public TagType Type_System;

    /// <summary>The TagType type.</summary>
    public TagType Type_TagType;

    /// <summary>The TernaryPass type.</summary>
    public TagType Type_TernayPass;

    /// <summary>The Text type.</summary>
    public TagType Type_Text;

    /// <summary>The Time type.</summary>
    public TagType Type_Time;

    /// <summary>Registers a type object for later usage by tags.</summary>
    /// <param name="type">The type object to register.</param>
    /// <param name="creator">The tag creator method (for SAVABLE data).</param>
    public void Register(TagType type, Func<string, TagData, TemplateObject> creator)
    {
        RegisteredTypes.Add(type.TypeName, type);
        SaveCreators[type.TypeName.ToLowerFast()] = creator;
        type.Engine = Engine;
    }

    /// <summary>Registers all the default tag types.</summary>
    public void RegisterDefaultTypes()
    {
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
        Register(Type_Dynamic = new TagType()
        {
            TypeName = DynamicTag.TYPE,
            SubTypeName = TextTag.TYPE,
            TypeGetter = DynamicTag.CreateFor,
            GetNextTypeDown = TextTag.For,
            SubHandlers = null,
            RawType = typeof(DynamicTag)
        }, DynamicTag.CreateFromSaved);
        Register(Type_Function = new TagType()
        {
            TypeName = FunctionTag.TYPE,
            SubTypeName = TextTag.TYPE,
            TypeGetter = FunctionTag.CreateFor,
            GetNextTypeDown = TextTag.For,
            SubHandlers = null,
            RawType = typeof(FunctionTag)
        }, (inp, dat) => FunctionTag.For(dat, inp));
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
            TypeGetter = ListTag.CreateFor,
            GetNextTypeDown = TextTag.For,
            SubHandlers = null,
            RawType = typeof(ListTag)
        }, ListTag.CreateFromSaved);
        Register(Type_Map = new TagType()
        {
            TypeName = MapTag.TYPE,
            SubTypeName = TextTag.TYPE,
            TypeGetter = MapTag.CreateFor,
            GetNextTypeDown = TextTag.For,
            SubHandlers = null,
            RawType = typeof(MapTag)
        }, MapTag.CreateFromSaved);
        Register(Type_Null = new TagType()
        {
            TypeName = NullTag.TYPE,
            SubTypeName = TextTag.TYPE,
            TypeGetter = (data, obj) => NullTag.NULL_VALUE,
            GetNextTypeDown = TextTag.For, // TODO: Or an error?
            SubHandlers = null,
            RawType = typeof(NullTag)
        }, (inp, dat) => NullTag.NULL_VALUE);
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
            TypeName = TernaryTagBase.TernaryPassTag.TYPE,
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
}
