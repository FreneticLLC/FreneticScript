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
using System.Reflection;
using System.Reflection.Emit;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.ScriptSystems
{
    /// <summary>
    /// Helper class to compile arguments.
    /// </summary>
    public static class ArgumentCompiler
    {
        /// <summary>
        /// The <see cref="Object.ToString"/> method.
        /// </summary>
        public static MethodInfo Object_ToString = typeof(object).GetMethod(nameof(Object.ToString), BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

        /// <summary>
        /// The <see cref="StringBuilder(int)"/> constructor.
        /// </summary>
        public static ConstructorInfo StringBuilder_CTOR = typeof(StringBuilder).GetConstructor(new Type[] { typeof(int) });

        /// <summary>
        /// The <see cref="StringBuilder.Append(string)"/> method.
        /// </summary>
        public static MethodInfo StringBuilder_Append = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);

        /// <summary>
        /// The <see cref="TextTag(string)"/> constructor.
        /// </summary>
        public static ConstructorInfo TextTag_CTOR = typeof(TextTag).GetConstructor(new Type[] { typeof(string) });

        /// <summary>
        /// The <see cref="Argument.RawBits"/> field.
        /// </summary>
        public static FieldInfo Argument_RawBits = typeof(Argument).GetField(nameof(Argument.RawBits));

        /// <summary>
        /// The <see cref="Argument.FirstBit"/> field.
        /// </summary>
        public static FieldInfo Argument_FirstBit = typeof(Argument).GetField(nameof(Argument.FirstBit));

        /// <summary>
        /// The <see cref="Argument.Parse(Action{string}, CompiledCommandRunnable)"/> method.
        /// </summary>
        public static MethodInfo Argument_Parse = typeof(Argument).GetMethod(nameof(Argument.Parse));

        /// <summary>
        /// The <see cref="ArgumentBit.Parse(Action{string}, CompiledCommandRunnable)"/> method.
        /// </summary>
        public static MethodInfo ArgumentBit_Parse = typeof(ArgumentBit).GetMethod(nameof(ArgumentBit.Parse));

        /// <summary>
        /// The <see cref="TextTag.EMPTY"/> field.
        /// </summary>
        public static FieldInfo TextTag_Empty = typeof(TextTag).GetField(nameof(TextTag.EMPTY));

        /// <summary>
        /// The <see cref="TextArgumentBit.InputValue"/> field.
        /// </summary>
        public static FieldInfo TextArgumentBit_InputValue = typeof(TextArgumentBit).GetField(nameof(TextArgumentBit.InputValue));

        private static long IDINCR = 0;

        /// <summary>
        /// All valid duplicator calls known. Null values indicate that there is known to not be a duplicator.
        /// </summary>
        public static Dictionary<Type, MethodInfo> DuplicatorCalls = new Dictionary<Type, MethodInfo>(128);

        /// <summary>
        /// Gets the resultant type of this argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public static TagType ReturnType(Argument argument, CILAdaptationValues values)
        {
            if (argument.Bits.Length == 1)
            {
                return argument.Bits[0].ReturnType(values);
            }
            else
            {
                return argument.Bits[0].Engine.TagSystem.Types.Type_Text;
            }
        }

        /// <summary>
        /// Compiles the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="entry">The relative stack entry.</param>
        public static void Compile(Argument argument, CompiledCommandStackEntry entry)
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
            MethodBuilder methodbuild_c = typebuild_c.DefineMethod("Parse", MethodAttributes.Public | MethodAttributes.Virtual, typeof(TemplateObject), new Type[] { typeof(Action<string>), typeof(CompiledCommandRunnable) });
            ILGeneratorTracker ilgen = new ILGeneratorTracker() { Internal = methodbuild_c.GetILGenerator(), System = entry.System };
            if (argument.Bits.Length == 0) // Empty argument
            {
                ilgen.Emit(OpCodes.Ldsfld, TextTag_Empty); // Load the empty texttag
            }
            else if (argument.Bits.Length == 1) // One argument input
            {
                int tab_tracker_loc = 0;
                int object_result_loc = 1;
                if (argument.FirstBit is TagArgumentBit)
                {
                    tab_tracker_loc = ilgen.DeclareLocal(typeof(TagArgumentBit)); // Declare variable of type TagArgumentBit as local-0
                    object_result_loc = ilgen.DeclareLocal(typeof(TemplateObject)); // Declare variable of type TemplateObject as local-1
                }
                ilgen.Emit(OpCodes.Ldarg_0); // Load the argument object
                ilgen.Emit(OpCodes.Ldfld, Argument_FirstBit); // Load the only argument bit
                if (argument.FirstBit is TextArgumentBit textab)
                {
                    ilgen.Emit(OpCodes.Ldfld, TextArgumentBit_InputValue); // Load the textab's input value directly
                }
                else if (argument.FirstBit is TagArgumentBit tab)
                {
                    tab.GenerateCall(ilgen, tab_tracker_loc, OpCodes.Ldarg_1, OpCodes.Ldarg_2, object_result_loc); // Call the tag - takes TAB on stack and adds a TemplateObject result onto it
                    ilgen.Emit(OpCodes.Ldloc, object_result_loc); // Load the object result
                }
                else
                {
                    ilgen.Emit(OpCodes.Ldarg_1); // Load Action<string> 'error'
                    ilgen.Emit(OpCodes.Ldarg_2); // Load CompiledCommandRunnable 'runnable'
                    ilgen.Emit(OpCodes.Callvirt, ArgumentBit_Parse); // Generic call to virtual parse method, for unknown argument bit types.
                }
            }
            else // Complex argument input
            {
                int result_string_loc = ilgen.DeclareLocal(typeof(StringBuilder)); // Declare variable of type StringBuilder as local-0
                bool hasTag = false;
                for (int i = 0; i < argument.Bits.Length; i++)
                {
                    if (argument.Bits[i] is TagArgumentBit tab)
                    {
                        hasTag = true;
                        break;
                    }
                }
                int object_result_loc = ilgen.DeclareLocal(typeof(TemplateObject)); // Declare variable of type TemplateObject as local-1
                int tab_tracker_loc = 0;
                if (hasTag)
                {
                    tab_tracker_loc = ilgen.DeclareLocal(typeof(TagArgumentBit)); // Declare variable of type TagArgumentBit as local-2
                }
                int cx = 2;
                for (int i = 0; i < argument.Bits.Length; i++)
                {
                    cx += argument.Bits[i].ToString().Length + 1;
                }
                ilgen.Emit(OpCodes.Ldc_I4, cx); // Load the integer value of the approximated length (to set an estimated capacity for the stringbuilder)
                ilgen.Emit(OpCodes.Newobj, StringBuilder_CTOR); // Construct an empty stringbuilder with that length
                ilgen.Emit(OpCodes.Stloc, result_string_loc); // Store the stringbuilder to a local variable
                for (int i = 0; i < argument.Bits.Length; i++)
                {
                    if (!(argument.Bits[i] is TagArgumentBit))
                    {
                        ilgen.Emit(OpCodes.Ldloc, result_string_loc); // Load the local variable containing the string builder
                    }
                    ilgen.Emit(OpCodes.Ldarg_0); // Load the argument object
                    ilgen.Emit(OpCodes.Ldfld, Argument_RawBits); // Load the bits array
                    ilgen.Emit(OpCodes.Ldc_I4, i); // Load the current index (will be a constant integer load at runtime)
                    ilgen.Emit(OpCodes.Ldelem_Ref); // Load the ArgumentBit
                    if (argument.Bits[i] is TagArgumentBit tab)
                    {
                        tab.GenerateCall(ilgen, tab_tracker_loc, OpCodes.Ldarg_1, OpCodes.Ldarg_2, object_result_loc); // Call the tag - takes TAB on stack and adds a TemplateObject result onto it
                        ilgen.Emit(OpCodes.Ldloc, result_string_loc); // Load the local variable containing the string builder
                        ilgen.Emit(OpCodes.Ldloc, object_result_loc); // Load the object result
                    }
                    else if (argument.Bits[i] is TextArgumentBit)
                    {
                        ilgen.Emit(OpCodes.Ldfld, TextArgumentBit_InputValue); // Load the tab's input value directly
                    }
                    else
                    {
                        ilgen.Emit(OpCodes.Ldarg_1); // Load Action<string> 'error'
                        ilgen.Emit(OpCodes.Ldarg_2); // Load CompiledCommandRunnable 'runnable'
                        ilgen.Emit(OpCodes.Callvirt, ArgumentBit_Parse); // Generic call to virtual parse method, for unknown argument bit types.
                    }
                    ilgen.Emit(OpCodes.Callvirt, Object_ToString); // Compress the result to a string
                    ilgen.Emit(OpCodes.Call, StringBuilder_Append); // Append it to the string builder
                    ilgen.Emit(OpCodes.Pop); // Remove the stringbuilder.append return value from the stack, as it's not needed (final assembly code should automatically discard the return push-n-pop)
                }
                ilgen.Emit(OpCodes.Ldloc, result_string_loc); // Load the stringbuilder
                ilgen.Emit(OpCodes.Callvirt, Object_ToString); // ToString it (maybe this doesn't need to be virtual?)
                ilgen.Emit(OpCodes.Newobj, TextTag_CTOR); // Construct a texttag of the full complex input
            }
            ilgen.Emit(OpCodes.Ret); // Return the resultant texttag or tag return value
            typebuild_c.DefineMethodOverride(methodbuild_c, Argument_Parse);
            Type t_c = typebuild_c.CreateType();
            argument.TrueForm = Activator.CreateInstance(t_c) as Argument;
            argument.TrueForm.Bits = argument.Bits;
            argument.TrueForm.WasQuoted = argument.WasQuoted;
            argument.TrueForm.CompiledParseMethod = methodbuild_c;
            argument.CompiledParseMethod = methodbuild_c;
        }
    }
}
