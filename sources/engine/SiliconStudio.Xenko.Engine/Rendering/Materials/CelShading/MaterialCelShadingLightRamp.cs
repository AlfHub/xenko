﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Default Cel Shading ramp function applied
    /// </summary>
    [DataContract("MaterialCelShadingLightRamp")]
    [Display("Ramp")]
    public class MaterialCelShadingLightRamp : IMaterialCelShadingLightFunction
    {
        /// <summary>
        /// The texture Reference.
        /// </summary>
        /// <userdoc>
        /// The reference to the texture asset to use.
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(null)]
        [Display("Ramp Texture")]
        public Texture RampTexture { get; set; }

        public ShaderSource Generate(MaterialGeneratorContext context) // (ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            // If we haven't specified a texture use the default implementation
            if (RampTexture == null)
                return new ShaderClassSource("MaterialCelShadingLightDefault", false);

            context.Material.Parameters.Set(MaterialCelShadingLightRampKeys.CelShaderRamp, RampTexture);

            return new ShaderClassSource("MaterialCelShadingLightRamp");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MaterialCelShadingLightRamp;
        }

        public override int GetHashCode()
        {
            return typeof(MaterialCelShadingLightRamp).GetHashCode();
        }
    }
}
