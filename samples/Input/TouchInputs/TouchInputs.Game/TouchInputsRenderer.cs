﻿using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;

namespace TouchInputs
{
    public class TouchInputsRenderer : SceneRendererBase
    {
        private SpriteBatch spriteBatch;

        private Vector2 virtualResolution = new Vector2(1920, 1080);

        public Texture Background;

        protected override void InitializeCore()
        {
            base.InitializeCore();

            // create the SpriteBatch used to render them
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = new Vector3(virtualResolution, 1000) };
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            // Clear
            context.CommandList.Clear(context.CommandList.RenderTarget, Color.Green);
            context.CommandList.Clear(context.CommandList.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);

            // Draw background
            spriteBatch.Begin(context.GraphicsContext);
            var target = context.CommandList.RenderTarget;
            var imageBufferMinRatio = Math.Min(Background.ViewWidth / (float)target.ViewWidth, Background.ViewHeight / (float)target.ViewHeight);
            var sourceSize = new Vector2(target.ViewWidth * imageBufferMinRatio, target.ViewHeight * imageBufferMinRatio);
            var source = new RectangleF((Background.ViewWidth - sourceSize.X) / 2, (Background.ViewHeight - sourceSize.Y) / 2, sourceSize.X, sourceSize.Y);
            spriteBatch.Draw(Background, new RectangleF(0, 0, virtualResolution.X, virtualResolution.Y), source, Color.White, 0, Vector2.Zero);
            spriteBatch.End();

            // Draw touch inputs
            var entity = context.RenderContext.SceneInstance.RootScene.Entities[0]; // Note: there's only one entity in our scene
            entity.Get<TouchInputsScript>().Render(context, spriteBatch);
        }
    }
}
