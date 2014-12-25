﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using System.Text;

using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Materials;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// A material blend layer
    /// </summary>
    [DataContract("MaterialBlendLayer")]
    [Display("Material Layer")]
    [ObjectFactory(typeof(Factory))]
    public class MaterialBlendLayer : IMaterialShaderGenerator
    {
        internal const string BlendStream = "matBlend";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendLayer"/> class.
        /// </summary>
        public MaterialBlendLayer()
        {
            Enabled = true;
            // Overrides = new MaterialBlendOverrides();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialBlendLayer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        [DataMember(10)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of this blend layer.
        /// </summary>
        /// <value>The name.</value>
        [DefaultValue(null)]
        [DataMember(20)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>The material.</value>
        [DefaultValue(null)]
        [DataMember(30)]
        public AssetReference<MaterialAsset> Material { get; set; }

        /// <summary>
        /// Gets or sets the blend map.
        /// </summary>
        /// <value>The blend map.</value>
        [Display("Blend Map")]
        [DefaultValue(null)]
        [DataMember(40)]
        public MaterialComputeColor BlendMap { get; set; }

        ///// <summary>
        ///// Gets or sets the material overrides.
        ///// </summary>
        ///// <value>The overrides.</value>
        //[DataMember(50)]
        //public MaterialBlendOverrides Overrides { get; private set; }

        private class Factory : IObjectFactory
        {
            public object New(Type type)
            {
                return new MaterialBlendLayer()
                {
                    BlendMap = new MaterialTextureComputeColor(),
                };
            }
        }

        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            // If not enabled, or Material or BlendMap are null, skip this layer
            if (!Enabled || Material == null || BlendMap == null)
            {
                return;
            }

            // Find the material from the reference
            var material = context.FindMaterial(Material);
            if (material == null)
            {
                context.Log.Error("Unable to find material [{0}]", Material);
                return;
            }

            // TODO: Because we are not fully supporting Streams declaration in shaders, we have to workaround this limitation by using a dynamic shader (inline)
            // TODO: Handle MaterialOverrides

            // Push a new stream stack for the sub-material
            context.PushStack();

            // Generate the material shaders into the current context
            material.GenerateShader(context);


            var isSameShadingModel = context.IsSameShadingModelAsParent;


            // Backup stream variables that will be modified by the materials
            var backupStreamBuilder = new StringBuilder();
            
            // Blend stream variables modified by the material with the previous backup
            var copyFromLayerBuilder = new StringBuilder();

            foreach (var stream in context.Streams)
            {
                backupStreamBuilder.AppendFormat("        var __backup__{0} = streams.{0};", stream).AppendLine();
            }

            // TODO: Hardcoded shading models is not good
            if (!isSameShadingModel)
            {
                // If the shading model of this layer is different from its parent, we are only blending the result of shading
                // Diffuse and Specular, instead of blending 
                backupStreamBuilder.AppendFormat("        var __backup__shadingDiffuse = streams.shadingDiffuse;").AppendLine();
                backupStreamBuilder.AppendFormat("        var __backup__shadingSpecular = streams.shadingSpecular;").AppendLine();
                backupStreamBuilder.AppendFormat("        streams.shadingDiffuse = 0;").AppendLine();
                backupStreamBuilder.AppendFormat("        streams.shadingSpecular = 0;").AppendLine();

                copyFromLayerBuilder.AppendFormat("        streams.shadingDiffuse = lerp(__backup__shadingDiffuse, streams.shadingDiffuse, streams.matBlend;").AppendLine();
                copyFromLayerBuilder.AppendFormat("        streams.shadingSpecular = lerp(__backup__shadingSpecular, streams.shadingSpecular, streams.matBlend;").AppendLine();
            }

            foreach (var stream in context.Streams)
            {
                if (isSameShadingModel)
                {
                    copyFromLayerBuilder.AppendFormat("        streams.{0} = lerp(__backup__{0}, streams.{0}, streams.matBlend;", stream).AppendLine();
                }
                else
                {
                    copyFromLayerBuilder.AppendFormat("        streams.{0} = __backup__{0};", stream).AppendLine();
                }
            }

            // Generate a dynamic shader name
            var shaderName = string.Format("MaterialBlendLayer{0}", context.NextId());
            var shaderClassSource = new ShaderClassSource(shaderName)
            {
                // The shader is an inline shader
                Inline = string.Format(DynamicBlendingShader, shaderName, backupStreamBuilder, copyFromLayerBuilder)
            };

            // Blend setup for this layer
            context.SetStream(BlendStream, BlendMap, MaterialKeys.BlendMap, MaterialKeys.BlendValue);
          
            // Generate the shader class source for the current stack
            var materialBlendLayerMixin = context.GenerateMixin();

            // Pop the stack
            context.PopStack();

            // Create a mixin
            var shaderMixinSource = new ShaderMixinSource();
            shaderMixinSource.Mixins.Add(shaderClassSource);

            // Add the shader to the mixin
            shaderMixinSource.AddComposition("subLayer", materialBlendLayerMixin);
            
            // Push the result of the shader mixin into the current stack
            context.AddSurfaceShader(shaderMixinSource);
        }

        private const string DynamicBlendingShader = @"
class {0} : IMaterialLayer
{{
    compose IMaterialLayer subLayer;

    override void Compute()
    {{
{1}        subLayer.Compute();
{2}}}
}};
";
    }
}