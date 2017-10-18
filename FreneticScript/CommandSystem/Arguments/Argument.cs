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
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// An Argument in the command system.
    /// </summary>
    public class Argument
    {
        /// <summary>
        /// The parts that build up the argument.
        /// </summary>
        public ArgumentBit[] Bits = new ArgumentBit[0];

        /// <summary>
        /// Whether the argument was input with "quotes" around it.
        /// </summary>
        public bool WasQuoted = true;
        
        /// <summary>
        /// Gets the resultant type of this argument.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public TagType ReturnType(CILAdaptationValues values)
        {
            if (Bits.Length == 1)
            {
                return Bits[0].ReturnType(values);
            }
            else
            {
                return Bits[0].CommandSystem.TagSystem.Type_Text;
            }
        }

        private static long IDINCR = 0;

        /// <summary>
        /// Compiles the argument.
        /// </summary>
        public void Compile()
        {
            string tname = "__script_argument__" + IDINCR++;
            AssemblyName asmname = new AssemblyName(tname) { Name = tname };
            AssemblyBuilder asmbuild = AppDomain.CurrentDomain.DefineDynamicAssembly(asmname,
#if NET_4_5
                    AssemblyBuilderAccess.RunAndCollect
#else
                    AssemblyBuilderAccess.Run
#endif
                    );
            ModuleBuilder modbuild = asmbuild.DefineDynamicModule(tname);
            TypeBuilder typebuild_c = modbuild.DefineType(tname + "__CENTRAL", TypeAttributes.Class | TypeAttributes.Public, typeof(Argument));
            MethodBuilder methodbuild_c = typebuild_c.DefineMethod("Parse", MethodAttributes.Public | MethodAttributes.Virtual, typeof(TemplateObject), new Type[] { typeof(Action<string>), typeof(CompiledCommandStackEntry) });
            CILAdaptationValues.ILGeneratorTracker ilgen = new CILAdaptationValues.ILGeneratorTracker() { Internal = methodbuild_c.GetILGenerator() };
            if (Bits.Length == 0)
            {
                ilgen.Emit(OpCodes.Ldstr, "");
                ilgen.Emit(OpCodes.Newobj, TextTag_CTOR);
            }
            else if (Bits.Length == 1)
            {
                ilgen.Emit(OpCodes.Ldarg_0);
                ilgen.Emit(OpCodes.Ldfld, Argument_Bits);
                ilgen.Emit(OpCodes.Ldc_I4, 0);
                ilgen.Emit(OpCodes.Ldelem_Ref);
                ilgen.Emit(OpCodes.Ldarg_1);
                ilgen.Emit(OpCodes.Ldarg_2);
                if (Bits[0] is TextArgumentBit)
                {
                    // TODO: Auto load relevant value?
                    ilgen.Emit(OpCodes.Call, TextArgumentBit_Parse);
                }
                else if (Bits[0] is TagArgumentBit)
                {
                    ilgen.Emit(OpCodes.Call, TagArgumentBit_Parse);
                }
                else
                {
                    ilgen.Emit(OpCodes.Callvirt, ArgumentBit_Parse);
                }
            }
            else
            {
                int ind = ilgen.DeclareLocal(typeof(StringBuilder));
                int cx = 0;
                for (int i = 0; i < Bits.Length; i++)
                {
                    cx += Bits[i].ToString().Length;
                }
                ilgen.Emit(OpCodes.Ldc_I4, cx);
                ilgen.Emit(OpCodes.Newobj, StringBuilder_CTOR);
                ilgen.Emit(OpCodes.Stloc, ind);
                for (int i = 0; i < Bits.Length; i++)
                {
                    ilgen.Emit(OpCodes.Ldloc, ind);
                    ilgen.Emit(OpCodes.Ldarg_0);
                    ilgen.Emit(OpCodes.Ldfld, Argument_Bits);
                    ilgen.Emit(OpCodes.Ldc_I4, i);
                    ilgen.Emit(OpCodes.Ldelem_Ref);
                    ilgen.Emit(OpCodes.Ldarg_1);
                    ilgen.Emit(OpCodes.Ldarg_2);
                    if (Bits[i] is TextArgumentBit)
                    {
                        // TODO: Auto load relevant value?
                        ilgen.Emit(OpCodes.Call, TextArgumentBit_Parse);
                    }
                    else if (Bits[i] is TagArgumentBit)
                    {
                        ilgen.Emit(OpCodes.Call, TagArgumentBit_Parse);
                    }
                    else
                    {
                        ilgen.Emit(OpCodes.Callvirt, ArgumentBit_Parse);
                    }
                    ilgen.Emit(OpCodes.Callvirt, Object_ToString);
                    ilgen.Emit(OpCodes.Call, StringBuilder_Append);
                    ilgen.Emit(OpCodes.Pop);
                }
                ilgen.Emit(OpCodes.Ldloc, ind);
                ilgen.Emit(OpCodes.Callvirt, Object_ToString);
                ilgen.Emit(OpCodes.Newobj, TextTag_CTOR);
            }
            ilgen.Emit(OpCodes.Ret);
            typebuild_c.DefineMethodOverride(methodbuild_c, Argument_Parse);
            Type t_c = typebuild_c.CreateType();
            TrueForm = Activator.CreateInstance(t_c) as Argument;
            TrueForm.Bits = Bits;
            TrueForm.WasQuoted = WasQuoted;
        }
        
        /// <summary>
        /// The ToString method on an object.
        /// </summary>
        public static MethodInfo Object_ToString = typeof(object).GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

        /// <summary>
        /// StringBuilder : constructor.
        /// </summary>
        public static ConstructorInfo StringBuilder_CTOR = typeof(StringBuilder).GetConstructor(new Type[] { typeof(int) });

        /// <summary>
        /// StringBuilder bit : append method.
        /// </summary>
        public static MethodInfo StringBuilder_Append = typeof(StringBuilder).GetMethod("Append", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);

        /// <summary>
        /// TextTag : constructor.
        /// </summary>
        public static ConstructorInfo TextTag_CTOR = typeof(TextTag).GetConstructor(new Type[] { typeof(string) });

        /// <summary>
        /// Argument : bits field.
        /// </summary>
        public static FieldInfo Argument_Bits = typeof(Argument).GetField("Bits");

        /// <summary>
        /// Argument : parse method.
        /// </summary>
        public static MethodInfo Argument_Parse = typeof(Argument).GetMethod("Parse");

        /// <summary>
        /// Argument bit : parse method.
        /// </summary>
        public static MethodInfo ArgumentBit_Parse = typeof(ArgumentBit).GetMethod("Parse");
        
        /// <summary>
        /// Text argument bit : parse method.
        /// </summary>
        public static MethodInfo TextArgumentBit_Parse = typeof(TextArgumentBit).GetMethod("Parse");

        /// <summary>
        /// Tag argument bit : parse method.
        /// </summary>
        public static MethodInfo TagArgumentBit_Parse = typeof(TagArgumentBit).GetMethod("Parse");

        /// <summary>
        /// The "true form" of this argument.
        /// </summary>
        public Argument TrueForm;

        /// <summary>
        /// Parse the argument, reading any tags or other special data.
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        /// <returns>The parsed final text.</returns>
        public virtual TemplateObject Parse(Action<string> error, CompiledCommandStackEntry cse)
        {
            if (TrueForm == null) // TODO: Remove need for this constant check!
            {
                Compile();
            }
            return TrueForm.Parse(error, cse);
        }

        /// <summary>
        /// Parse the argument, reading any tags or other special data.
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        /// <returns>The parsed final text.</returns>
        public TemplateObject ParseSlow(Action<string> error, CompiledCommandStackEntry cse)
        {
            if (Bits.Length == 1)
            {
                return Bits[0].Parse(error, cse);
            }
            StringBuilder built = new StringBuilder();
            for (int i = 0; i < Bits.Length; i++)
            {
                built.Append(Bits[i].Parse(error, cse).ToString());
            }
            return new TextTag(built.ToString());
        }

        /// <summary>
        /// Returns the argument as plain input text.
        /// </summary>
        /// <returns>The plain input text.</returns>
        public override string ToString()
        {
            if (Bits.Length == 1)
            {
                return Bits[0].ToString();
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Bits.Length; i++)
            {
                sb.Append(Bits[i].ToString());
            }
            return sb.ToString();
        }
    }
}
