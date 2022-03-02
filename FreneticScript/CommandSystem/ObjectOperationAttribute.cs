//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem
{

    /// <summary>Represents an object operation method.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ObjectOperationAttribute : Attribute
    {
        /// <summary>The type of operation.</summary>
        public ObjectOperation Operation;

        /// <summary>The input tag type (if any).</summary>
        public string Input;

        /// <summary>The method this is attached to.</summary>
        public MethodInfo Method;

        /// <summary>A function call for the method, based on objects.</summary>
        public Func<TemplateObject, TemplateObject, ObjectEditSource, TemplateObject> ObjectFunc;

        /// <summary>A function call for the method, using a string as input.</summary>
        public Func<TemplateObject, string, ObjectEditSource, TemplateObject> StringFunc;

        /// <summary>A function call for the method, using a string-set-action setup.</summary>
        public Action<TemplateObject, TemplateObject, string, ObjectEditSource> SetFunc;

        private static readonly Type[] GET_FUNC_PARAMS = new Type[] { typeof(TemplateObject), typeof(string), typeof(ObjectEditSource) };
        private static readonly Type[] SET_FUNC_PARAMS = new Type[] { typeof(TemplateObject), typeof(TemplateObject), typeof(string), typeof(ObjectEditSource) };
        private static readonly Type[] OBJECT_FUNC_PARAMS = new Type[] { typeof(TemplateObject), typeof(TemplateObject), typeof(ObjectEditSource) };

        /// <summary>Generate the operation function helper objects.</summary>
        public void GenerateFunctions()
        {
            if (Operation == ObjectOperation.GETSUBSETTABLE)
            {
                ParameterInfo[] methodParams = Method.GetParameters();
                DynamicMethod genMethod = new("operation_" + Operation + "_callable_tag_" + Method.DeclaringType.Name + "_" + Method.Name,
                    typeof(TemplateObject), GET_FUNC_PARAMS);
                ILGenerator opILGen = genMethod.GetILGenerator();
                opILGen.Emit(OpCodes.Ldarg_0); // Load argument: TemplateObject.
                if (methodParams[0].ParameterType != typeof(TemplateObject))
                {
                    opILGen.Emit(OpCodes.Castclass, methodParams[0].ParameterType); // Convert it to the correct type
                }
                opILGen.Emit(OpCodes.Ldarg_1); // Load argument: string
                if (methodParams.Length == 3)
                {
                    opILGen.Emit(OpCodes.Ldarg_2); // Load argument: ObjectEditSource.
                }
                opILGen.Emit(OpCodes.Call, Method); // Run the actual operation method.
                opILGen.Emit(OpCodes.Ret); // Return.
                StringFunc = genMethod.CreateDelegate(typeof(Func<TemplateObject, string, ObjectEditSource, TemplateObject>))
                    as Func<TemplateObject, string, ObjectEditSource, TemplateObject>;
            }
            else if (Operation == ObjectOperation.SET)
            {
                ParameterInfo[] methodParams = Method.GetParameters();
                DynamicMethod genMethod = new("operation_" + Operation + "_callable_tag_" + Method.DeclaringType.Name + "_" + Method.Name,
                    typeof(void), SET_FUNC_PARAMS);
                ILGenerator opILGen = genMethod.GetILGenerator();
                opILGen.Emit(OpCodes.Ldarg_0); // Load argument: TemplateObject 1.
                if (methodParams[0].ParameterType != typeof(TemplateObject))
                {
                    opILGen.Emit(OpCodes.Castclass, methodParams[0].ParameterType); // Convert it to the correct type
                }
                opILGen.Emit(OpCodes.Ldarg_1); // Load argument: TemplateObject 2.
                if (methodParams[0].ParameterType != typeof(TemplateObject))
                {
                    opILGen.Emit(OpCodes.Castclass, methodParams[1].ParameterType); // Convert it to the correct type
                }
                opILGen.Emit(OpCodes.Ldarg_2); // Load argument: string
                if (methodParams.Length == 4)
                {
                    opILGen.Emit(OpCodes.Ldarg_3); // Load argument: ObjectEditSource.
                }
                opILGen.Emit(OpCodes.Call, Method); // Run the actual operation method.
                opILGen.Emit(OpCodes.Ret); // Return.
                SetFunc = genMethod.CreateDelegate(typeof(Action<TemplateObject, TemplateObject, string, ObjectEditSource>))
                    as Action<TemplateObject, TemplateObject, string, ObjectEditSource>;
            }
            else
            {
                ParameterInfo[] methodParams = Method.GetParameters();
                DynamicMethod genMethod = new("operation_" + Operation + "_callable_tag_" + Method.DeclaringType.Name + "_" + Method.Name,
                    typeof(TemplateObject), OBJECT_FUNC_PARAMS);
                ILGenerator opILGen = genMethod.GetILGenerator();
                opILGen.Emit(OpCodes.Ldarg_0); // Load argument: TemplateObject 1.
                if (methodParams[0].ParameterType != typeof(TemplateObject))
                {
                    opILGen.Emit(OpCodes.Castclass, methodParams[0].ParameterType); // Convert it to the correct type
                }
                opILGen.Emit(OpCodes.Ldarg_1); // Load argument: TemplateObject 2.
                if (methodParams[1].ParameterType != typeof(TemplateObject))
                {
                    opILGen.Emit(OpCodes.Castclass, methodParams[1].ParameterType); // Convert it to the correct type
                }
                if (methodParams.Length == 3)
                {
                    opILGen.Emit(OpCodes.Ldarg_2); // Load argument: ObjectEditSource.
                }
                opILGen.Emit(OpCodes.Call, Method); // Run the actual operation method.
                opILGen.Emit(OpCodes.Ret); // Return.
                ObjectFunc = genMethod.CreateDelegate(typeof(Func<TemplateObject, TemplateObject, ObjectEditSource, TemplateObject>))
                    as Func<TemplateObject, TemplateObject, ObjectEditSource, TemplateObject>;
            }
        }

        /// <summary>Constructs the object operation attribute.</summary>
        /// <param name="_operation">The operation type.</param>
        public ObjectOperationAttribute(ObjectOperation _operation)
        {
            Operation = _operation;
        }
    }
}
