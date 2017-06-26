// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Navigation.Tests
{
    public class DynamicBarrierTest : Game
    {
        private Entity entityA;
        private Entity entityB;
        private PlayerController controllerA;
        private PlayerController controllerB;

        private Entity filterB;
        private Entity filterAB;

        private Vector3 targetPosition = new Vector3(1.4f, 0.0f, 0.0f);

        private DynamicNavigationMeshSystem dynamicNavigation;

        public DynamicBarrierTest()
        {
            AutoLoadDefaultSettings = true;
            IsDrawDesynchronized = false;
            IsFixedTimeStep = true;
            ForceOneUpdatePerDraw = true;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            entityA = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "A");
            entityB = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "B");

            entityA.Add(controllerA = new PlayerController());
            entityB.Add(controllerB = new PlayerController());

            filterAB = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "FilterAB");
            filterB = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "FilterB");

            dynamicNavigation = (DynamicNavigationMeshSystem)GameSystems.FirstOrDefault(x => x is DynamicNavigationMeshSystem);
            if (dynamicNavigation == null)
                throw new Exception("Failed to find dynamic navigation mesh system");

            dynamicNavigation.AutomaticRebuild = false;
            dynamicNavigation.Enabled = true;

            Script.AddTask(RunAsyncTests);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (gameTime.Total > TimeSpan.FromSeconds(6))
            {
                Assert.Fail("Test timed out");
            }
        }

        private async Task RunAsyncTests()
        {
            // Wait for start method to be called
            while(controllerA.Character == null)
                await Script.NextFrame();

            // Wait for controllers to be on the ground
            while (!controllerA.Character.IsGrounded || !controllerB.Character.IsGrounded)
                await Script.NextFrame();

            controllerA.UpdateSpawnPosition();
            controllerB.UpdateSpawnPosition();

            // Enabled a wall that blocks A and B
            RecursiveToggle(filterAB, true);
            RecursiveToggle(filterB, false);
            var buildResult = await dynamicNavigation.Rebuild();
            Assert.IsTrue(buildResult.Success);
            Assert.AreEqual(2, buildResult.UpdatedLayers.Count);

            await Task.WhenAll(controllerA.TryMove(targetPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }),
                controllerB.TryMove(targetPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }));

            await Reset();

            // Enabled a wall that only blocks B
            RecursiveToggle(filterAB, false);
            RecursiveToggle(filterB, true);
            buildResult = await dynamicNavigation.Rebuild();
            Assert.IsTrue(buildResult.Success);

            await Task.WhenAll(controllerA.TryMove(targetPosition).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }),
                controllerB.TryMove(targetPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }));

            await Reset();

            // Disable both walls
            RecursiveToggle(filterAB, false);
            RecursiveToggle(filterB, false);
            buildResult = await dynamicNavigation.Rebuild();
            Assert.IsTrue(buildResult.Success);

            await Task.WhenAll(controllerA.TryMove(targetPosition).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }),
                controllerB.TryMove(targetPosition).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }));

            // Walk back to spawn with only letting A pass
            RecursiveToggle(filterAB, false);
            RecursiveToggle(filterB, true);
            buildResult = await dynamicNavigation.Rebuild();
            Assert.IsTrue(buildResult.Success);

            await Task.WhenAll(controllerA.TryMove(controllerA.SpawnPosition).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }),
                controllerB.TryMove(controllerB.SpawnPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }));

            Exit();
        }

        private async Task Reset()
        {
            controllerA.Reset();
            controllerB.Reset();
            await Script.NextFrame();
        }

        private void RecursiveToggle(Entity entity, bool enabled)
        {
            var model = entity.Get<ModelComponent>();
            if (model != null)
                model.Enabled = enabled;
            var collider = entity.Get<StaticColliderComponent>();
            if (collider != null)
                collider.Enabled = enabled;

            foreach (var c in entity.GetChildren())
                RecursiveToggle(c, enabled);
        }

        [Test]
        public static void DynamicBarrierTest1()
        {
            DynamicBarrierTest game = new DynamicBarrierTest();
            game.Run();
            game.Dispose();
        }
        
        public static void Main()
        {
            DynamicBarrierTest1();
        }
    }
}
