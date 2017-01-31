// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// Base class for shadow map renderers
    /// </summary>
    [DataContract(Inherited = true, DefaultMemberMode = DataMemberMode.Never)]
    public abstract class LightShadowMapRendererBase : ILightShadowMapRenderer
    {
        /// <summary>
        /// The shadow map render stage this light shadow map renderer uses
        /// </summary>
        [DataMember]
        public RenderStage ShadowCasterRenderStage { get; set; }

        public abstract void Reset(RenderContext context);

        public virtual LightShadowType GetShadowType(LightShadowMap shadowMap)
        {
            // TODO: MOVE THIS TO BASE TYPE
            var shadowType = (LightShadowType)0;
            switch (shadowMap.GetCascadeCount())
            {
                case 1:
                    shadowType |= LightShadowType.Cascade1;
                    break;
                case 2:
                    shadowType |= LightShadowType.Cascade2;
                    break;
                case 4:
                    shadowType |= LightShadowType.Cascade4;
                    break;
            }

            var pcfFilter = shadowMap.Filter as LightShadowMapFilterTypePcf;
            if (pcfFilter != null)
            {
                switch (pcfFilter.FilterSize)
                {
                    case LightShadowMapFilterTypePcfSize.Filter3x3:
                        shadowType |= LightShadowType.PCF3x3;
                        break;
                    case LightShadowMapFilterTypePcfSize.Filter5x5:
                        shadowType |= LightShadowType.PCF5x5;
                        break;
                    case LightShadowMapFilterTypePcfSize.Filter7x7:
                        shadowType |= LightShadowType.PCF7x7;
                        break;
                }
            }

            if (shadowMap.Debug)
            {
                shadowType |= LightShadowType.Debug;
            }
            return shadowType;
        }

        public abstract ILightShadowMapShaderGroupData CreateShaderGroupData(LightShadowType shadowType);

        public abstract bool CanRenderLight(IDirectLight light);

        public abstract void Collect(RenderContext context, RenderView sourceView, LightShadowMapTexture lightShadowMap);

        public abstract void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, LightShadowMapTexture shadowMapTexture);

        public abstract LightShadowMapTexture CreateTexture(LightComponent lightComponent, IDirectLight light, int shadowMapSize);
    }
}