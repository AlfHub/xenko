﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A user defined stream.
    /// </summary>
    /// <userdoc>
    /// A user defined stream.
    /// </userdoc>
    [DataContract("VertexUserStreamDefinition")]
    [Display("Custom Vertex Stream")]
    public class VertexUserStreamDefinition : VertexStreamDefinitionBase
    {
        private string name;

        private int hashCode;

        public override int GetHashCode() => hashCode;

        public VertexUserStreamDefinition()
        {
            name = "COLOR";
            hashCode = name.GetHashCode();
        }

        /// <summary>
        /// Name of the semantic of the stream to read data from.
        /// </summary>
        /// <userdoc>
        /// Semantic name of the stream to read data from.
        /// </userdoc>
        [DataMember(10)]
        public string Name {
            get { return name; }
            set { name = value; hashCode = name.GetHashCode(); }
        }

        public override string GetSemanticName()
        {
            return name;
        }

        public override int GetSemanticNameHash()
        {
            return hashCode;
        }
    }
}