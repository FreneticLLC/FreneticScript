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
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;

namespace FreneticScript.TagHandlers
{
    /// <summary>An abstract class, implementations are used as tag bases.</summary>
    public abstract class TemplateTagBase
    {
        /// <summary>Constructs the tag base.</summary>
        public TemplateTagBase()
        {
            Type t = GetType();
            Method_HandleOne = t.GetMethod("HandleOne", BindingFlags.Public | BindingFlags.Static);
            Method_HandleOneObjective = t.GetMethod("HandleOneObjective", BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>Represents the HandleOne(...) method.</summary>
        public MethodInfo Method_HandleOne;

        /// <summary>Represents the HandleOne(...) method.</summary>
        public MethodInfo Method_HandleOneObjective;

        /// <summary>The name of the tag base.</summary>
        public string Name = null;

        /// <summary>What type this tag handler will return. Default for dynamic.</summary>
        public TagReturnType ResultType = default;

        /// <summary>The name of the type this tag handler will return. Null for dynamic.</summary>
        public string ResultTypeString = null;

        /// <summary>Adapts the tag base to CIL.</summary>
        /// <param name="ilgen">IL Generator.</param>
        /// <param name="tab">The TagArgumentBit.</param>
        /// <param name="values">Related adaptation values.</param>
        /// <returns>Whether any adaptation was done.</returns>
        public virtual bool AdaptToCIL(ILGeneratorTracker ilgen, TagArgumentBit tab, CILAdaptationValues values)
        {
            return false;
        }

        /// <summary>Adapts the template tab base for compiling.</summary>
        /// <param name="ccse">The compiled CSE.</param>
        /// <param name="tab">The TagArgumentBit.</param>
        /// <param name="i">The command index.</param>
        /// <param name="values">Related adaptation values.</param>
        public virtual TagReturnType Adapt(CompiledCommandStackEntry ccse, TagArgumentBit tab, int i, CILAdaptationValues values)
        {
            return default;
        }

        /// <summary>Usually returns the name of this tag base.</summary>
        /// <returns>The name of this tag base.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
