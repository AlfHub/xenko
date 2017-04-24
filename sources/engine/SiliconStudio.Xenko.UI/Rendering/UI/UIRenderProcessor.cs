// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Rendering.UI
{
    /// <summary>
    /// The processor in charge of updating and drawing the entities having sprite components.
    /// </summary>
    public class UIRenderProcessor : EntityProcessor<UIComponent, RenderUIElement>, IEntityComponentRenderProcessor
    {
        public List<RenderUIElement> UIRoots { get; private set; }

        public VisibilityGroup VisibilityGroup { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UIRenderProcessor"/> class.
        /// </summary>
        public UIRenderProcessor()
            : base(typeof(TransformComponent))
        {
            UIRoots = new List<RenderUIElement>();
        }
        
        public override void Draw(RenderContext gameTime)
        {
            UIRoots.Clear();
            foreach (var spriteStateKeyPair in ComponentDatas)
            {
                var renderUIElement = spriteStateKeyPair.Value;
                renderUIElement.Enabled = renderUIElement.UIComponent.Enabled;

                if (renderUIElement.Enabled)
                {
                    // TODO GRAPHICS REFACTOR: Proper bounding box.
                    //renderSprite.BoundingBox = new BoundingBoxExt(new Vector3(float.NegativeInfinity), new Vector3(float.PositiveInfinity));
                    renderUIElement.RenderGroup = renderUIElement.UIComponent.RenderGroup;

                    UIRoots.Add(renderUIElement);
                }
            }
        }

        protected override void OnEntityComponentAdding(Entity entity, UIComponent uiComponent, RenderUIElement renderUIElement)
        {
            VisibilityGroup.RenderObjects.Add(renderUIElement);
        }

        protected override void OnEntityComponentRemoved(Entity entity, UIComponent uiComponent, RenderUIElement renderUIElement)
        {
            VisibilityGroup.RenderObjects.Remove(renderUIElement);
        }

        protected override RenderUIElement GenerateComponentData(Entity entity, UIComponent component)
        {
            return new RenderUIElement(component, entity.Transform) { RenderGroup = component.RenderGroup };
        }

        protected override bool IsAssociatedDataValid(Entity entity, UIComponent component, RenderUIElement associatedData)
        {
            return
                entity.Get<UIComponent>() == component &&
                entity.Transform == associatedData.TransformComponent;
        }
    }
}
