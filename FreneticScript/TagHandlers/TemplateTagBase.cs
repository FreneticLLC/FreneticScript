using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// An abstract class, implementations are used as tag bases.
    /// </summary>
    public abstract class TemplateTagBase
    {
        /// <summary>
        /// Constructs the tag base.
        /// </summary>
        public TemplateTagBase()
        {
            Type t = GetType();
            Method_HandleOne = t.GetMethod("HandleOne", BindingFlags.Public | BindingFlags.Static);
            Method_HandleOneObjective = t.GetMethod("HandleOneObjective", BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Represents the HandleOne(...) method.
        /// </summary>
        public MethodInfo Method_HandleOne;

        /// <summary>
        /// Represents the HandleOne(...) method.
        /// </summary>
        public MethodInfo Method_HandleOneObjective;

        /// <summary>
        /// The name of the tag base.
        /// </summary>
        public string Name = null;
        
        /// <summary>
        /// What type this tag handler will return. Null for dynamic.
        /// </summary>
        public TagType ResultType = null;

        /// <summary>
        /// The name of the type this tag handler will return. Null for dynamic.
        /// </summary>
        public string ResultTypeString = null;
        
        /// <summary>
        /// Adapts the template tab base for compiling.
        /// </summary>
        /// <param name="ccse">The compiled CSE.</param>
        /// <param name="tab">The TagArgumentBit.</param>
        /// <param name="i">The command index.</param>
        /// <param name="a">The argument index.</param>
        public virtual TagType Adapt(CompiledCommandStackEntry ccse, TagArgumentBit tab, int i, int a)
        {
            return null;
        }
        
        /// <summary>
        /// Usually returns the name of this tag base.
        /// </summary>
        /// <returns>The name of this tag base.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
