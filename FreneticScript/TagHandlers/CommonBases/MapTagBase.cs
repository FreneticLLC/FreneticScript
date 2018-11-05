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
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.CommonBases
{
    // <--[explanation]
    // @Name Maps
    // @Description
    // A map is a relationship between textual names and object values.
    // TODO: Explain better!
    // -->

    /// <summary>
    /// Handles the 'map' tag base.
    /// </summary>
    public class MapTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base map[<MapTag>]
        // @Group Mathematics
        // @ReturnType MapTag
        // @Returns the specified input as a map.
        // <@link explanation maps>What are maps?<@/link>
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public MapTagBase()
        {
            Name = "map";
            ResultTypeString = MapTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return MapTag.For(data.GetModifierObjectCurrent(), data);
        }
    }
}
