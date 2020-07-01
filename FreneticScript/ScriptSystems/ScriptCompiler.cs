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
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;
using FreneticScript.TagHandlers.Objects;
using System.Runtime.CompilerServices;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticScript.ScriptSystems
{
    /// <summary>
    /// Helper class to compile scripts.
    /// </summary>
    public static class ScriptCompiler
    {
        private static readonly Type[] RUN_METHOD_PARAMETERS = new Type[] { typeof(CommandQueue) };

        /// <summary>
        /// Compiles a command script.
        /// </summary>
        /// <param name="script">The command script to compile.</param>
        /// <returns>The compiled result.</returns>
        public static CompiledCommandStackEntry Compile(CommandScript script)
        {
            string tname = "__script__" + IDINCR++ + "__" + NameTrimMatcher.TrimToMatches(script.Name);
            CompiledCommandStackEntry Created = new CompiledCommandStackEntry()
            {
                Entries = script.CommandArray,
                Script = script,
                AssemblyName = tname
            };
            AssemblyName asmname = new AssemblyName(tname) { Name = tname };
            AssemblyBuilder asmbuild = AppDomain.CurrentDomain.DefineDynamicAssembly(asmname,
#if NET_4_5
                AssemblyBuilderAccess.RunAndCollect
#else
                AssemblyBuilderAccess.Run
#endif
                );
            ModuleBuilder modbuild = asmbuild.DefineDynamicModule(tname);
            CompiledCommandStackEntry ccse = Created;
            ccse.AdaptedILPoints = new Label[ccse.Entries.Length + 1];
            TypeBuilder typebuild_c = modbuild.DefineType(tname + "__CENTRAL", TypeAttributes.Class | TypeAttributes.Public, typeof(CompiledCommandRunnable));
            MethodBuilder methodbuild_c = typebuild_c.DefineMethod("Run", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), RUN_METHOD_PARAMETERS);
            ILGeneratorTracker ilgen = new ILGeneratorTracker() { Internal = methodbuild_c.GetILGenerator(), System = Created.System };
            ilgen.AddCode(OpCodes.Nop, tname, "--- SCRIPT ---");
            CILAdaptationValues values = new CILAdaptationValues()
            {
                Entry = ccse,
                Script = script,
                ILGen = ilgen,
                Method = methodbuild_c,
                DBMode = script.Debug,
                EntryFields = new FieldInfo[ccse.Entries.Length],
                ArgumentFields = new FieldInfo[ccse.Entries.Length][],
                Type = typebuild_c
            };
            for (int i = 0; i < ccse.Entries.Length; i++)
            {
                values.EntryFields[i] = typebuild_c.DefineField("_field_entry_" + i, typeof(CommandEntry), FieldAttributes.Public | FieldAttributes.InitOnly);
            }
            values.PushVarSet();
            for (int i = 0; i < ccse.AdaptedILPoints.Length; i++)
            {
                ccse.AdaptedILPoints[i] = ilgen.DefineLabel();
            }
            int tagID = 0;
            List<TagArgumentBit> toClean = new List<TagArgumentBit>();
            List<ILGeneratorTracker> ILGens
#if SAVE
                = new List<ILGeneratorTracker>();
#else
                = null;
#endif
            values.Trackers = ILGens;
            List<KeyValuePair<FieldInfo, Object>> specialFieldValues = new List<KeyValuePair<FieldInfo, object>>();
            for (int i = 0; i < ccse.Entries.Length; i++)
            {
                CommandEntry curEnt = ccse.Entries[i];
                curEnt.CCSE = ccse;
                curEnt.DBMode = values.DBMode;
                curEnt.VarLookup = values.CreateVarLookup();
                for (int a = 0; a < curEnt.Arguments.Length; a++)
                {
                    Argument arg = curEnt.Arguments[a];
                    for (int b = 0; b < arg.Bits.Length; b++)
                    {
                        if (arg.Bits[b] is TagArgumentBit tab)
                        {
                            tagID++;
                            try
                            {
                                GenerateTagData(typebuild_c, ccse, tab, ref tagID, values, i, toClean, (a + 1).ToString(), curEnt, specialFieldValues, ILGens);
                            }
                            catch (TagErrorInducedException ex)
                            {
                                TagException(curEnt, "argument " + TextStyle.Separate + a + TextStyle.Base, tab, ex.SubTagIndex, ex);
                            }
                            catch (ErrorInducedException ex)
                            {
                                TagException(curEnt, "argument " + TextStyle.Separate + a + TextStyle.Base, tab, 0, ex);
                            }
                        }
                    }
                    ILGeneratorTracker compiled = ArgumentCompiler.Compile(arg, Created, values);
                    curEnt.Arguments[a] = arg.TrueForm;
                    ILGens?.Add(compiled);
                }
                foreach (KeyValuePair<string, Argument> argPair in curEnt.NamedArguments)
                {
                    for (int b = 0; b < argPair.Value.Bits.Length; b++)
                    {
                        if (argPair.Value.Bits[b] is TagArgumentBit tab)
                        {
                            tagID++;
                            try
                            {
                                GenerateTagData(typebuild_c, ccse, tab, ref tagID, values, i, toClean, "named " + argPair.Key, curEnt, specialFieldValues, ILGens);
                            }
                            catch (TagErrorInducedException ex)
                            {
                                TagException(curEnt, "named argument '" + TextStyle.Separate + argPair.Key + TextStyle.Base + "'", tab, ex.SubTagIndex, ex);
                            }
                            catch (ErrorInducedException ex)
                            {
                                TagException(curEnt, "named argument '" + TextStyle.Separate + argPair.Key + TextStyle.Base + "'", tab, 0, ex);
                            }
                        }
                    }
                    ILGeneratorTracker compiled = ArgumentCompiler.Compile(argPair.Value, Created, values);
                    ILGens?.Add(compiled);
                }
                if (!curEnt.IsCallback)
                {
                    try
                    {
                        curEnt.Command.PreAdaptToCIL(values, i);
                    }
                    catch (ErrorInducedException ex)
                    {
                        throw new ErrorInducedException("On script line " + curEnt.ScriptLine + " (" + curEnt.CommandLine + "), early compile (PreAdapt) error occured: " + ex.Message);
                    }
                    curEnt.VarLookup = values.CreateVarLookup();
                }
                if (curEnt.NamedArguments.TryGetValue(CommandEntry.SAVE_NAME_ARG_ID, out Argument avarname))
                {
                    if (!curEnt.VarLookup.ContainsKey(avarname.ToString()))
                    {
                        throw new ErrorInducedException("Error in command line " + curEnt.ScriptLine + ": (" + curEnt.CommandLine + "): Invalid variable save name: " + avarname.ToString());
                    }
                }
                if (curEnt.IsCallback)
                {
                    try
                    {
                        curEnt.Command.PreAdaptToCIL(values, i);
                    }
                    catch (ErrorInducedException ex)
                    {
                        throw new ErrorInducedException("On script line " + curEnt.ScriptLine + " (" + curEnt.CommandLine + "), early compile (PreAdapt) error occured: " + ex.Message);
                    }
                }
                curEnt.DBMode = values.DBMode;
            }
            values.LoadRunnable();
            ilgen.Emit(OpCodes.Ldfld, CompiledCommandRunnable.IndexField);
            ilgen.Emit(OpCodes.Switch, ccse.AdaptedILPoints);
            for (int i = 0; i < ccse.Entries.Length; i++)
            {
                ilgen.MarkLabel(ccse.AdaptedILPoints[i]);
                try
                {
                    ccse.Entries[i].Command.AdaptToCIL(values, i);
                }
                catch (ErrorInducedException ex)
                {
                    throw new ErrorInducedException("On script line " + ccse.Entries[i].ScriptLine + " (" + ccse.Entries[i].CommandLine + "), compile error (Adapt) occured: " + ex.Message);
                }
            }
            ilgen.MarkLabel(ccse.AdaptedILPoints[ccse.AdaptedILPoints.Length - 1]);
            values.MarkCommand(ccse.Entries.Length);
            ilgen.Emit(OpCodes.Ret);
            typebuild_c.DefineMethodOverride(methodbuild_c, CompiledCommandRunnable.RunMethod);
            ConstructorBuilder ctor = typebuild_c.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, CONSTRUCTOR_PARAMS);
            ILGeneratorTracker ctorilgen = new ILGeneratorTracker() { Internal = ctor.GetILGenerator(), System = ccse.System };
            ILGens?.Add(ctorilgen);
            ctorilgen.Emit(OpCodes.Ldarg_0); // Load 'this'
            ctorilgen.Emit(OpCodes.Ldarg_1); // Load: CCSE
            ctorilgen.Emit(OpCodes.Stfld, CompiledCommandRunnable.EntryField); // Store it to the readonly field.
            for (int i = 0; i < values.EntryFields.Length; i++)
            {
                ctorilgen.Emit(OpCodes.Ldarg_0); // Load 'this'
                ctorilgen.Emit(OpCodes.Ldarg_2); // Load input array
                ctorilgen.Emit(OpCodes.Ldc_I4, i); // Load index in the array
                ctorilgen.Emit(OpCodes.Ldelem_Ref); // Load the value from the array
                ctorilgen.Emit(OpCodes.Stfld, values.EntryFields[i]); // Store it to the readonly field.
                FieldInfo[] argFields = values.ArgumentFields[i];
                if (argFields != null)
                {
                    for (int arg = 0; arg < argFields.Length; arg++)
                    {
                        if (argFields[arg] != null)
                        {
                            ctorilgen.Emit(OpCodes.Ldarg_0); // Load 'this'
                            ctorilgen.Emit(OpCodes.Ldarg_0); // Load 'this'
                            ctorilgen.Emit(OpCodes.Ldfld, values.EntryFields[i]); // Load the entry field
                            ctorilgen.Emit(OpCodes.Ldfld, CILAdaptationValues.Entry_ArgumentsField); // Load the arguments field
                            ctorilgen.Emit(OpCodes.Ldc_I4, arg); // Load the argument index
                            ctorilgen.Emit(OpCodes.Ldelem_Ref); // Load the argument value from the array
                            ctorilgen.Emit(OpCodes.Stfld, argFields[arg]); // Store it to the readonly field.
                        }
                    }
                }
            }
            for (int i = 0; i < specialFieldValues.Count; i++)
            {
                ctorilgen.Emit(OpCodes.Ldarg_0); // Load 'this'
                ctorilgen.Emit(OpCodes.Ldarg_3); // Load input array
                ctorilgen.Emit(OpCodes.Ldc_I4, i); // Load index in the array
                ctorilgen.Emit(OpCodes.Ldelem_Ref); // Load the value from the array
                ctorilgen.Emit(OpCodes.Castclass, specialFieldValues[i].Key.FieldType); // Guarantee the type.
                ctorilgen.Emit(OpCodes.Stfld, specialFieldValues[i].Key); // Store it to the field.
            }
            ctorilgen.Emit(OpCodes.Ret); // return
            Type t_c = typebuild_c.CreateType();
            object[] fieldValues = new object[specialFieldValues.Count];
            for (int i = 0; i < fieldValues.Length; i++)
            {
                fieldValues[i] = specialFieldValues[i].Value;
            }
            CompiledCommandRunnable runnable = Activator.CreateInstance(t_c, ccse, ccse.Entries, fieldValues) as CompiledCommandRunnable;
            ccse.ReferenceCompiledRunnable = runnable;
            ccse.Variables = values.Variables.ToArray();
            foreach (SingleCILVariable variable in ccse.Variables)
            {
                variable.Field = variable.Field.DeclaringType.GetField(variable.Field.Name); // Patch runtime field issues
            }
            ccse.VariableSetters = new Action<CompiledCommandRunnable, TemplateObject>[ccse.Variables.Length];
            runnable.EntryData = new AbstractCommandEntryData[Created.Entries.Length];
            runnable.Debug = script.Debug;
#if SAVE
            StringBuilder outp = new StringBuilder();
            for (int i = 0; i < ilgen.Codes.Count; i++)
            {
                outp.Append(ilgen.Codes[i].Key + ": " + ilgen.Codes[i].Value?.ToString()?.Replace("\n", "\\n")?.Replace("\r", "\\r") + "\n");
            }
            for (int n = 0; n < ILGens.Count; n++)
            {
                outp.Append("\n\n\n// -----\n\n\n");
                for (int i = 0; i < ILGens[n].Codes.Count; i++)
                {
                    outp.Append(ILGens[n].Codes[i].Key + ": " + ILGens[n].Codes[i].Value?.ToString()?.Replace("\n", "\\n")?.Replace("\r", "\\r") + "\n");
                }
            }
            System.IO.File.WriteAllText("script_" + tname + ".il", outp.ToString());
#endif
            return Created;
        }

        private static readonly Type[] CONSTRUCTOR_PARAMS = new Type[] { typeof(CompiledCommandStackEntry), typeof(CommandEntry[]), typeof(Object[]) };


        private static readonly Type[] SETTER_ACTION_PARAMS = new Type[] { typeof(CompiledCommandRunnable), typeof(TemplateObject) };

        /// <summary>
        /// Creates a variable setter for the given variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns>The setter action.</returns>
        public static Action<CompiledCommandRunnable, TemplateObject> CreateVariableSetter(SingleCILVariable variable)
        {
            DynamicMethod genMethod = new DynamicMethod("script_" + variable.Field.DeclaringType.Name + "_var_" + variable.Index + "_setter", typeof(void), SETTER_ACTION_PARAMS);
            ILGeneratorTracker ILGen = new ILGeneratorTracker() { Internal = genMethod.GetILGenerator(), System = variable.Type.Type.Engine };
            ILGen.Emit(OpCodes.Ldarg_0); // Load argument: runnable
            ILGen.Emit(OpCodes.Castclass, variable.Field.DeclaringType); // Jank patch for type misinterpretation in CILL validator
            ILGen.Emit(OpCodes.Ldarg_1); // Load argument: input variable value
            ILGen.Emit(OpCodes.Ldsfld, TagData.FIELD_TAGDATA_SIMPLE_ERROR); // Grab a blank TagData
            ILGen.Emit(OpCodes.Call, variable.Type.Type.CreatorMethod); // Ensure type
            if (variable.Type.IsRaw)
            {
                ILGen.Emit(OpCodes.Ldfld, variable.Type.Type.RawInternalField); // Handle raw if needed.
            }
            ILGen.Emit(OpCodes.Stfld, variable.Field); // Store to field.
            ILGen.Emit(OpCodes.Ret); // Return.
            return genMethod.CreateDelegate(typeof(Action<CompiledCommandRunnable, TemplateObject>)) as Action<CompiledCommandRunnable, TemplateObject>;
        }

        /// <summary>
        /// Matcher for usable script name characters.
        /// </summary>
        public static AsciiMatcher NameTrimMatcher = new AsciiMatcher((c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));

        /// <summary>
        /// Throws a tag failure exception.
        /// </summary>
        /// <param name="entry">Relevant command entry.</param>
        /// <param name="argumentNote">Note for the argument, like: "in named argument 'fail'."</param>
        /// <param name="tab">Relevant tag.</param>
        /// <param name="tagIndex">Index of failure bit within the tag.</param>
        /// <param name="ex">Source exception.</param>
        public static void TagException(CommandEntry entry, string argumentNote, TagArgumentBit tab, int tagIndex, ErrorInducedException ex)
        {
            throw new ErrorInducedException("On script line " + TextStyle.Separate + entry.ScriptLine
                + TextStyle.Base + " (" + TextStyle.Separate + entry.CommandLine
                + TextStyle.Base + "), in " + argumentNote
                + " while compiling tag " + TextStyle.Separate +
                "<" + tab.HighlightString(tagIndex, TextStyle.Warning) + TextStyle.Separate + ">"
                + TextStyle.Base + ", error occured: " + ex.Message);
        }

        private static readonly Type[] TYPES_TAGPARSE_PARAMS = new Type[] { typeof(TagData), typeof(CompiledCommandRunnable), typeof(Action<string>) };
        
        /// <summary>
        /// Generates tag CIL.
        /// </summary>
        /// <param name="typeBuild_c">The type to contain this tag.</param>
        /// <param name="ccse">The CCSE available.</param>
        /// <param name="tab">The tag data.</param>
        /// <param name="tID">The ID of the tag.</param>
        /// <param name="values">The helper values.</param>
        /// <param name="entryIndex">The command entry index.</param>
        /// <param name="toClean">Cleanable tag bits.</param>
        /// <param name="argumentId">Source command argument ID.</param>
        /// <param name="commandEntry">The source command entry.</param>
        /// <param name="specialFieldValues">Field values to set on the runnable.</param>
        /// <param name="trackers">All generator trackers thus far.</param>
        public static void GenerateTagData(TypeBuilder typeBuild_c, CompiledCommandStackEntry ccse, TagArgumentBit tab,
            ref int tID, CILAdaptationValues values, int entryIndex, List<TagArgumentBit> toClean, string argumentId, CommandEntry commandEntry,
            List<KeyValuePair<FieldInfo, Object>> specialFieldValues, List<ILGeneratorTracker> trackers)
        {
            int id = tID;
            // Build a list of sub-arguments (within the tag) that may need to be compiled
            List<Argument> altArgs = new List<Argument>();
            for (int sub = 0; sub < tab.Bits.Length; sub++)
            {
                if (tab.Bits[sub].Variable != null)
                {
                    altArgs.Add(tab.Bits[sub].Variable);
                }
            }
            if (tab.Fallback != null)
            {
                altArgs.Add(tab.Fallback);
            }
            // Compile any sub-arguments
            for (int sx = 0; sx < altArgs.Count; sx++)
            {
                for (int b = 0; b < altArgs[sx].Bits.Length; b++)
                {
                    if (altArgs[sx].Bits[b] is TagArgumentBit subTab)
                    {
                        tID++;
                        GenerateTagData(typeBuild_c, ccse, subTab, ref tID, values, entryIndex, toClean, argumentId, commandEntry, specialFieldValues, trackers);
                    }
                }
                ILGeneratorTracker tracker = ArgumentCompiler.Compile(altArgs[sx], ccse, values);
                trackers?.Add(tracker);
            }
            // Basic setup
            if (tab.Start == null)
            {
                TagBit firstBit = tab.Bits[0];
                if ((firstBit.Variable == null || firstBit.Variable.Bits.Length == 0) && commandEntry.VarLookup.TryGetValue(firstBit.Key, out SingleCILVariable startVar))
                {
                    firstBit.OriginalInput = firstBit.Key;
                    tab.Start = tab.TagSystem.LVar;
                    firstBit.Key = "\0lvar";
                    firstBit.Variable = new Argument() { WasQuoted = false, Bits = new ArgumentBit[] { new TextArgumentBit(startVar.Index, tab.Engine) } };
                }
                else
                {
                    throw new TagErrorInducedException("Invalid tag base '" + TextStyle.Separate + firstBit.Key + TextStyle.Base + "'!");
                }
            }
            if (tab.Start == null)
            {
                throw new TagErrorInducedException($"Invalid tag base '{TextStyle.Separate}{tab.Bits[0].Key}{TextStyle.Base}' (unknown)!", 0);
            }
            TagReturnType returnable = tab.Start.ResultType;
            if (returnable.Type == null)
            {
                returnable = tab.Start.Adapt(ccse, tab, entryIndex, values);
            }
            if (returnable.Type == null)
            {
                throw new TagErrorInducedException($"Invalid tag top-handler '{TextStyle.Separate}{tab.Start.Name}{TextStyle.Base}' (failed to identify return type)!", 0);
            }
            TagReturnType prevType = returnable;
            for (int x = 1; x < tab.Bits.Length; x++)
            {
                string key = tab.Bits[x].Key;
                if (!returnable.Type.TagHelpers.ContainsKey(key))
                {
                    if (returnable.Type.TagHelpers.ContainsKey("_"))
                    {
                        key = "_";
                        goto ready;
                    }
                    TagReturnType basicType = returnable;
                    while (returnable.Type.SubType != null)
                    {
                        returnable = new TagReturnType(returnable.Type.SubType, false);
                        if (returnable.Type.TagHelpers.ContainsKey(key))
                        {
                            goto ready;
                        }
                    }
                    throw new TagErrorInducedException("Invalid sub-tag '"
                        + TextStyle.Separate + key + TextStyle.Base + "' at sub-tag index "
                        + TextStyle.Separate + x + TextStyle.Base + " for type '"
                        + TextStyle.Separate + basicType.Type.TypeName + TextStyle.Base
                        + (key.Trim().Length == 0 ? "' (stray '.' dot symbol?)!" : "' (sub-tag doesn't seem to exist)!"), x);
                }
            ready:
                TagHelpInfo tsh = returnable.Type.TagHelpers[key];
                tab.Bits[x].TagHandler = tsh;
                if (tsh.Meta.ReturnTypeResult.Type == null)
                {
                    if (tab.Bits[x].TagHandler.Meta.SpecialTypeHelper != null)
                    {
                        returnable = tab.Bits[x].TagHandler.Meta.SpecialTypeHelper(tab, x);
                    }
                    else
                    {
                        throw new TagErrorInducedException("Invalid tag ReturnType '" + TextStyle.Separate + tsh.Meta.ReturnType
                            + TextStyle.Base + " for tag '" + TextStyle.Separate + tsh.Meta.ActualType.TypeName + "."
                            + TextStyle.Separate + tsh.Meta.Name + TextStyle.Base + "', cannot process properly!", x);
                    }
                }
                else
                {
                    returnable = tsh.Meta.ReturnTypeResult;
                }
            }
            int vxLen = tab.Bits.Length;
            Argument[] varBits = new Argument[vxLen];
            for (int vxi = 0; vxi < vxLen; vxi++)
            {
                varBits[vxi] = tab.Bits[vxi].Variable;
            }
            CommandEntry relevantEntry = ccse.Entries[entryIndex];
            tab.Data = new TagData()
            {
                BaseColor = TextStyle.Simple,
                cInd = 0,
                Runnable = null,
                ErrorHandler = null,
                Fallback = tab.Fallback,
                Bits = tab.Bits,
                Variables = varBits,
                DBMode = relevantEntry.DBMode,
                Start = tab.Start,
                TagSystem = tab.TagSystem,
                SourceArgumentID = argumentId
            };
            TagReturnType finalReturnType = tab.ReturnType(values);
            // Build a method that handles the tag.
            string methodName = "TagParse_" + id + "_" + ccse.AssemblyName + "_Line_" + commandEntry.ScriptLine + "_Arg_" + NameTrimMatcher.TrimToMatches(argumentId) + "_" +
                string.Join("_", tab.Bits.Select((bit) => NameTrimMatcher.TrimToMatches(bit.Key)));
            Type methodReturnType = finalReturnType.IsRaw ? finalReturnType.Type.RawInternalType : typeof(TemplateObject);
            MethodBuilder methodbuild_c = typeBuild_c.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, methodReturnType, TYPES_TAGPARSE_PARAMS);
            ILGeneratorTracker ilgen = new ILGeneratorTracker() { Internal = methodbuild_c.GetILGenerator(), System = commandEntry.System };
            ilgen.AddCode(OpCodes.Nop, methodName, "--- TAGPARSE ---");
            trackers?.Add(ilgen);
            if (!tab.Start.AdaptToCIL(ilgen, tab, values))
            {
                ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData
                ilgen.Emit(OpCodes.Ldc_I4_0); // Load a '0' int32
                ilgen.Emit(OpCodes.Stfld, TagData.Field_cInd); // Store the '0' into TagData.cInd
                if (tab.Start.Method_HandleOneObjective != null) // If objective tag handling...
                {
                    ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData.
                    ilgen.Emit(OpCodes.Ldfld, TagData.Field_Start); // Load field TagData -> Start.
                    ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData.
                    ilgen.Emit(OpCodes.Call, tab.Start.Method_HandleOneObjective); // Run instance method: TemplateTagBase -> HandleOneObjective.
                }
                else // If faster static tag handling
                {
                    ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData.
                    ilgen.Emit(OpCodes.Call, tab.Start.Method_HandleOne); // Run static method: TemplateTagBase -> HandleOne.
                }
            }
            for (int x = 1; x < tab.Bits.Length; x++)
            {
                ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData
                ilgen.Emit(OpCodes.Ldc_I4, x); // Load the current sub-tag index as an int32
                ilgen.Emit(OpCodes.Stfld, TagData.Field_cInd); // Store x into TagData.cInd
                // If we're running a specially compiled tag...
                if (tab.Bits[x].TagHandler.Meta.SpecialCompiler)
                {
                    prevType = tab.Bits[x].TagHandler.Meta.SpecialCompileAction(ilgen, tab, x, prevType);
                }
                else // For normal tags...
                {
                    while (tab.Bits[x].TagHandler.Meta.ActualType != prevType.Type)
                    {
                        if (prevType.IsRaw)
                        {
                            ilgen.Emit(OpCodes.Newobj, prevType.Type.RawInternalConstructor); // Handle raw translation if needed.
                        }
                        ilgen.Emit(OpCodes.Call, prevType.Type.GetNextTypeDown.Method);
                        prevType = new TagReturnType(prevType.Type.SubType, false);
                        if (prevType.Type == null)
                        {
                            throw new Exception("Failed to parse down a tag: type reached the base type without finding the expected tag type! (Compiler bug?)"
                                + " Processing tag " + tab + " on bit " + x);
                        }
                    }
                    if (prevType.IsRaw && !tab.Bits[x].TagHandler.Meta.SelfIsRaw)
                    {
                        ilgen.Emit(OpCodes.Newobj, prevType.Type.RawInternalConstructor); // Handle raw translation if needed.
                    }
                    else if (!prevType.IsRaw && tab.Bits[x].TagHandler.Meta.SelfIsRaw)
                    {
                        ilgen.Emit(OpCodes.Ldfld, prevType.Type.RawInternalField); // Handle raw translation if needed.
                    }
                    prevType = tab.Bits[x].TagHandler.Meta.ReturnTypeResult;
                    TagReturnType modt = tab.Bits[x].TagHandler.Meta.ModifierType;
                    if (modt.Type != null) // If we have a modifier input type pre-requirement...
                    {
                        Argument variableArg = tab.Bits[x].Variable.TrueForm;
                        tab.Bits[x].Variable = variableArg;
                        TagReturnType atype = ArgumentCompiler.ReturnType(variableArg, values);
                        if (variableArg.Bits.Length == 0)
                        {
                            ilgen.Emit(OpCodes.Ldsfld, ArgumentCompiler.TextTag_Empty); // Load the empty texttag
                        }
                        else if (variableArg.Bits.Length == 1 && variableArg.Bits[0] is TextArgumentBit text)
                        {
                            FieldBuilder argValueField = typeBuild_c.DefineField("_field_tag_line_" + entryIndex + "_" + id + "_argVal_" + x, text.InputValue.GetType(), FieldAttributes.Public | FieldAttributes.InitOnly);
                            specialFieldValues.Add(new KeyValuePair<FieldInfo, object>(argValueField, text.InputValue));
                            ilgen.Emit(OpCodes.Ldarg_1); // Load argument: Runnable.
                            ilgen.Emit(OpCodes.Ldfld, argValueField); // Load the modifier argument value
                            atype.IsRaw = false; // TODO: Store correct raw value in the field
                        }
                        else
                        {
                            FieldBuilder argToParse = typeBuild_c.DefineField("_field_tag_line_" + entryIndex + "_" + id + "_arg_" + x, typeof(Argument), FieldAttributes.Public | FieldAttributes.InitOnly);
                            specialFieldValues.Add(new KeyValuePair<FieldInfo, object>(argToParse, variableArg));
                            ilgen.Emit(OpCodes.Ldarg_1); // Load argument: Runnable.
                            ilgen.Emit(OpCodes.Ldfld, argToParse); // Load the modifier argument
                            ilgen.Emit(OpCodes.Ldarg_2); // Load argument: Error.
                            ilgen.Emit(OpCodes.Ldarg_1); // Load argument: Runnable.
                            ilgen.Emit(OpCodes.Call, variableArg.RawParseMethod ?? variableArg.CompiledParseMethod, 2); // Call the modifier Parse(Error, Runnable) method.
                        }
                        Tag_MethodEnsureType(ilgen, atype, modt); // Automatic type correction
                    }
                    else
                    {
                        ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData.
                    }
                    ilgen.Emit(OpCodes.Call, tab.Bits[x].TagHandler.Method); // Run the tag's own runner method.
                }
            }
            if (relevantEntry.DBMode <= DebugMode.FULL) // If debug mode is on...
            {
                if (prevType.IsRaw)
                {
                    ilgen.Emit(OpCodes.Newobj, prevType.Type.RawInternalConstructor); // Handle raw translation if needed.
                }
                ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData.
                ilgen.Emit(OpCodes.Call, TagHandler.Method_DebugTagHelper); // Debug the tag as a final step. Will give back the object to the stack.
                if (prevType.IsRaw)
                {
                    ilgen.Emit(OpCodes.Ldfld, prevType.Type.RawInternalField); // Handle raw translation if needed.
                }
            }
            ilgen.Emit(OpCodes.Ret); // Return.
#if NET_4_5
            methodbuild_c.SetCustomAttribute(new CustomAttributeBuilder(Ctor_MethodImplAttribute_Options, Input_Params_AggrInline));
#endif
            tab.GetResultMethod = methodbuild_c;
            tab.CompiledReturnType = prevType;
            toClean.Add(tab);
        }

        /// <summary>
        /// Emits logic that ensures the argument is of the given type, converting if needed.
        /// </summary>
        /// <param name="ilgen">The IL generator.</param>
        /// <param name="currentType">The current object type.</param>
        /// <param name="requiredType">The type it needs to be.</param>
        public static void Tag_MethodEnsureType(ILGeneratorTracker ilgen, TagReturnType currentType, TagReturnType requiredType)
        {
            if (currentType.Type != requiredType.Type)
            {
                if (currentType.IsRaw)
                {
                    ilgen.Emit(OpCodes.Newobj, currentType.Type.RawInternalConstructor);
                }
                ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData
                ilgen.Emit(OpCodes.Call, requiredType.Type.CreatorMethod);
                if (requiredType.IsRaw)
                {
                    ilgen.Emit(OpCodes.Ldfld, requiredType.Type.RawInternalField);
                }
            }
            else if (currentType.IsRaw && !requiredType.IsRaw)
            {
                ilgen.Emit(OpCodes.Newobj, currentType.Type.RawInternalConstructor);
            }
            else if (!currentType.IsRaw && requiredType.IsRaw)
            {
                ilgen.Emit(OpCodes.Ldfld, currentType.Type.RawInternalField);
            }
        }

#if NET_4_5
        /// <summary>
        /// The <see cref="MethodImplAttribute(MethodImplOptions)"/> constructor.
        /// </summary>
        public static readonly ConstructorInfo Ctor_MethodImplAttribute_Options = typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(MethodImplOptions) });

        /// <summary>
        /// A reusable input object array that contains <see cref="MethodImplOptions.AggressiveInlining"/>.
        /// </summary>
        public static readonly object[] Input_Params_AggrInline = new object[] { MethodImplOptions.AggressiveInlining };
#endif
        
        /// <summary>
        /// Generates a dynamically callable method for a tag.
        /// </summary>
        /// <param name="method">The tag method.</param>
        /// <param name="meta">The tag method.</param>
        /// <param name="system">The relevant script engine.</param>
        /// <returns>The callable.</returns>
        public static Func<TemplateObject, TagData, TemplateObject> GenerateTagMethodCallable(MethodInfo method, TagMeta meta, ScriptEngine system)
        {
            if (meta.SpecialCompiler)
            {
                return null;
            }
            try
            {
                DynamicMethod genMethod = new DynamicMethod("tag_parse_for_" + method.DeclaringType.Name + "_" + method.Name, typeof(TemplateObject), new Type[] { typeof(TemplateObject), typeof(TagData) });
                ILGeneratorTracker ilgen = new ILGeneratorTracker() { Internal = genMethod.GetILGenerator(), System = system };
                ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TemplateObject.
                ilgen.Emit(OpCodes.Castclass, method.GetParameters()[0].ParameterType); // Convert it to the correct type
                if (meta.ModifierType.Type != null)
                {
                    ilgen.Emit(OpCodes.Ldarg_1); // Load argument: TagData.
                    ilgen.Emit(OpCodes.Call, TagData.Method_GetModifierObjectCurrent); // Call the method to get the tag modifier object at the current location.
                    ilgen.Emit(OpCodes.Ldarg_1); // Load argument: TagData.
                    ilgen.Emit(OpCodes.Call, meta.ModifierType.Type.CreatorMethod); // Run the creator method to convert the tag to the correct type if needed.
                    if (meta.ModifierType.IsRaw)
                    {
                        ilgen.Emit(OpCodes.Ldfld, meta.ModifierType.Type.RawInternalField); // Handle raw translation if needed.
                    }
                }
                else
                {
                    ilgen.Emit(OpCodes.Ldarg_1); // Load argument: TagData.
                }
                ilgen.Emit(OpCodes.Call, method); // Run the tag's own runner method.
                if (meta.ReturnTypeResult.IsRaw)
                {
                    ilgen.Emit(OpCodes.Newobj, meta.ReturnTypeResult.Type.RawInternalConstructor); // Handle raw translation if needed.
                }
                ilgen.Emit(OpCodes.Ret); // Return.
#if SAVE
                StringBuilder outp = new StringBuilder();
                for (int i = 0; i < ilgen.Codes.Count; i++)
                {
                    outp.Append(ilgen.Codes[i].Key + ": " + ilgen.Codes[i].Value + "\n");
                }
                System.IO.Directory.CreateDirectory("debug_tags");
                System.IO.File.WriteAllText("debug_tags/script_" + genMethod.Name + ".il", outp.ToString());
#endif
                return genMethod.CreateDelegate(typeof(Func<TemplateObject, TagData, TemplateObject>)) as Func<TemplateObject, TagData, TemplateObject>;
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Failed to compile tag: '" + method.DeclaringType + "." + method.Name + "' (tag: '" + meta.TagType + "." + meta.Name + "'), because: " + ex.Message, ex);
            }
        }
        
        /// <summary>
        /// The method <see cref="Func{TagData, TemplateObject, TemplateObject}.Invoke(TagData, TemplateObject)"/> with typeparams <see cref="TagData"/>, <see cref="TemplateObject"/>, <see cref="TemplateObject"/>.
        /// </summary>
        public static readonly MethodInfo Method_Func_TD_TO_TO_Invoke = typeof(Func<TagData, TemplateObject, TemplateObject>).GetMethod(nameof(Func<TagData, TemplateObject, TemplateObject>.Invoke));

        /// <summary>
        /// Incrementing ID value for method compilation.
        /// </summary>
        public static long IDINCR = 0;
    }

    /// <summary>
    /// Represents a tag type returned from a tag method.
    /// </summary>
    public struct TagReturnType
    {
        /// <summary>
        /// Constructs a <see cref="TagReturnType"/> instance.
        /// </summary>
        /// <param name="_type">The returned tag type.</param>
        /// <param name="_isRaw">Whether the type is raw.</param>
        public TagReturnType(TagType _type, bool _isRaw = false)
        {
            Type = _type;
            IsRaw = _isRaw;
        }

        /// <summary>
        /// The returned tag type.
        /// </summary>
        public TagType Type;

        /// <summary>
        /// Whether the type is raw.
        /// </summary>
        public bool IsRaw;
    }
}
