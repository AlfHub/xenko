﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Tests
{
    /// <summary>
    /// Tests for <see cref="TransformComponent"/>.
    /// </summary>
    [TestFixture]
    public class TestTransformComponent
    {
        /// <summary>
        /// Test conversions between entity local/world space
        /// </summary>
        [Test]
        public void TestWorldAndLocalSpace()
        {
            var entity = new Entity();
            var trans = entity.Transform;

            // Make sure that an entity has a transform component
            Assert.NotNull(trans);
            Assert.AreEqual(1, entity.Components.Count);
            Assert.AreEqual(entity.Transform, entity.Components[0]);

            // Test point to world/local space conversion
            trans.Position = new Vector3(1, 2, 3);
            trans.UpdateWorldMatrix();
            Assert.AreEqual(new Vector3(1, 2, 3), trans.LocalToWorld(new Vector3(0, 0, 0)));
            Assert.AreEqual(new Vector3(4, 4, 4), trans.LocalToWorld(new Vector3(3, 2, 1)));
            Assert.AreEqual(new Vector3(-1, -2, -3), trans.WorldToLocal(new Vector3(0, 0, 0)));
            Assert.AreEqual(new Vector3(0, 0, 0), trans.WorldToLocal(new Vector3(1, 2, 3)));
            trans.Position = new Vector3(1, 0, 0);
            trans.Rotation = Quaternion.RotationX((float)Math.PI * 0.5f);
            trans.Scale = new Vector3(2, 2, 2);
            trans.UpdateWorldMatrix();
            Assert.AreEqual(new Vector3(1, 0, 2), trans.LocalToWorld(new Vector3(0, 1, 0)));
            Vector3 tP1 = new Vector3(0, 0, 0);
            Quaternion tR1 = new Quaternion(0, 0, 0, 1);
            Vector3 tS1 = new Vector3(1, 1, 1);
            trans.WorldToLocal(ref tP1, ref tR1, ref tS1);
            Assert.AreEqual(new Vector3(-0.5f, 0, 0), tP1);
            Assert.AreEqual(Quaternion.RotationX((float)Math.PI * -0.5f), tR1);
            Assert.AreEqual(new Vector3(0.5f, 0.5f, 0.5f), tS1);
        }
    }
}