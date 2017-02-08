﻿// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.NextGen;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering.Compositing;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    public class TestLightShafts : GraphicTestGameBase
    {
        public TestLightShafts()
        {
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };
            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            //GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.Debug;
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();

            SceneSystem.InitialSceneUrl = "LightShafts";
            SceneSystem.GraphicsCompositor = GraphicsCompositor.CreateDefault(true);
            var fwr = ((SceneSystem.GraphicsCompositor.Game as SceneCameraRenderer).Child as ForwardRenderer);
            if (fwr.PostEffects != null)
            {
                fwr.PostEffects = new PostProcessingEffects();
                fwr.PostEffects.LightShafts.Enabled = true;
                fwr.PostEffects.DepthOfField.Enabled = false;
                fwr.PostEffects.AmbientOcclusion.Enabled = false;
                fwr.PostEffects.Antialiasing.Enabled = false;
                fwr.PostEffects.BrightFilter.Enabled = false;
                fwr.PostEffects.Bloom.Enabled = false;
                fwr.PostEffects.LensFlare.Enabled = false;
                fwr.PostEffects.ColorTransforms.Transforms.Add(new ToneMap());
            }
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            ProfilerSystem.EnableProfiling(false, GameProfilingKeys.GameDrawFPS);

            Window.AllowUserResizing = true;

            var cameraEntity = SceneSystem.SceneInstance.First(x => x.Get<CameraComponent>() != null);
            cameraEntity.Add(new FpsTestCamera());
            cameraEntity.Transform.Position = new Vector3(0.0f, 5.0f, 10.0f);
            cameraEntity.Transform.Rotation = Quaternion.Identity;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public static void Main()
        {
            using (var game = new TestLightShafts())
                game.Run();
        }
    }
}