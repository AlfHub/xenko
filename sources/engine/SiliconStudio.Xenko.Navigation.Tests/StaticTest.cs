﻿// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Navigation.Tests
{
    public class StaticTest : Game
    {
        public Vector3 targetA = new Vector3(1.2f, 0.0f, -1.0f);
        public Vector3 targetB = new Vector3(1.2f, 0.0f, 1.0f);

        private Entity entityA;
        private Entity entityB;
        private PlayerController controllerA;
        private PlayerController controllerB;

        public StaticTest()
        {
            AutoLoadDefaultSettings = true;
        }

        protected override void PrepareContext()
        {
            base.PrepareContext();
            SceneSystem.InitialSceneUrl = "StaticTest";
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            entityA = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "A");
            entityB = SceneSystem.SceneInstance.RootScene.Entities.FirstOrDefault(x => x.Name == "B");

            entityA.Add(controllerA = new PlayerController());
            entityB.Add(controllerB = new PlayerController());

            var dynamicNavigation = (DynamicNavigationMeshSystem)GameSystems.FirstOrDefault(x => x is DynamicNavigationMeshSystem);
            if(dynamicNavigation != null)
                dynamicNavigation.Enabled = false;

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
            while (controllerA.Character == null)
                await Script.NextFrame();

            // Wait for controllers to be on the ground
            while (!controllerA.Character.IsGrounded || !controllerB.Character.IsGrounded)
                await Script.NextFrame();

            controllerA.UpdateSpawnPosition();
            controllerB.UpdateSpawnPosition();

            // Move to lower box
            await Task.WhenAll(controllerA.TryMove(targetB).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }),
                controllerB.TryMove(targetB).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }));

            // Move to upper box
            await Task.WhenAll(controllerA.TryMove(targetA).ContinueWith(x => { Assert.IsTrue(x.Result.Success); }),
                controllerB.TryMove(targetA).ContinueWith(x => { Assert.IsFalse(x.Result.Success); }));

            // Change group of A to the group that B has
            controllerA.Navigation.Group = controllerB.Navigation.Group;

            // Move A to it's spawn (should fail with the new group)
            await controllerA.TryMove(controllerA.SpawnPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); });

            // Remove B's navigation mesh
            controllerB.Navigation.NavigationMesh = null;

            // Move B to it's spawn (should fail as well now)
            await controllerB.TryMove(controllerB.SpawnPosition).ContinueWith(x => { Assert.IsFalse(x.Result.Success); });

            Exit();
        }

        [Test]
        public static void StaticTest1()
        {
            StaticTest game = new StaticTest();
            game.Run();
            game.Dispose();
        }

        public static void Main()
        {
            StaticTest1();
        }
    }
}