using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Sprites
{
    public class SpriteTransparentRenderStageSelector : TransparentRenderStageSelector
    {
        public override void Process(RenderObject renderObject)
        {
            if (((RenderGroupMask)(1U << (int)renderObject.RenderGroup) & RenderGroup) != 0)
            {
                var renderSprite = (RenderSprite)renderObject;

                var renderStage = renderSprite.SpriteComponent.CurrentSprite.IsTransparent ? TransparentRenderStage : MainRenderStage;
                renderObject.ActiveRenderStages[renderStage.Index] = new ActiveRenderStage(EffectName);
            }
        }
    }
}