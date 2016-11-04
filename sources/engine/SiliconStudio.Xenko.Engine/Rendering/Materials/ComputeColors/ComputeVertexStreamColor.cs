﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A compute color producing a color from a stream.
    /// </summary>
    [DataContract("ComputeVertexStreamColor")]
    [Display("Vertex Stream")]
    public class ComputeVertexStreamColor : ComputeVertexStreamBase, IComputeColor
    {
        private int oldHashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeVertexStreamColor"/> class.
        /// </summary>
        public ComputeVertexStreamColor()
        {
            Stream = new ColorVertexStreamDefinition();
            oldHashCode = 0;
        }

        protected override string GetColorChannelAsString()
        {
            // Use all channels
            return "rgba";
        }

        /// <inheritdoc/>
        public bool HasChanged
        {
            get
            {
                if (oldHashCode != 0 && oldHashCode == Stream.GetSemanticNameHash())
                    return false;

                oldHashCode = Stream.GetSemanticNameHash();
                return true;
            }
        }
    }
}