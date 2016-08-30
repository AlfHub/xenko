﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ContentPresenter"/> class.
    /// </summary>
    [TestFixture, Ignore("ContentPresenter is deprecated.")]
    [System.ComponentModel.Description("Tests for ContentPresenter layering")]
    public class ContentPresenterTests : ContentPresenter
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            var newButton = new Button();

            // - test the properties that are supposed to invalidate the object measurement
            UIElementLayeringTests.TestMeasureInvalidation(this, () => Content = newButton);

            var sameButton = newButton;

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => Content = sameButton);
        }

        /// <summary>
        /// Test the update of the world matrix of children invalidation
        /// </summary>
        [Test]
        public void TestUpdateWorldMatrixInvalidation()
        {
            var children = new Button();
            Content = children;

            var worldMatrix = Matrix.Zero;
            var localMatrix = Matrix.Identity;

            Measure(Vector3.Zero);
            Arrange(Vector3.Zero, false);
            UpdateWorldMatrix(ref worldMatrix, false);

            worldMatrix.M11 = 2;
            UpdateWorldMatrix(ref worldMatrix, true);
            Assert.AreEqual(worldMatrix.M11, children.WorldMatrix.M11);

            worldMatrix.M11 = 3;
            UpdateWorldMatrix(ref worldMatrix, false);
            Assert.AreEqual(2, children.WorldMatrix.M11);

            worldMatrix.M11 = 1;
            localMatrix.M11 = 4;
            LocalMatrix = localMatrix;
            UpdateWorldMatrix(ref worldMatrix, false);
            Assert.AreEqual(localMatrix.M11, children.WorldMatrix.M11);

            localMatrix.M11 = 1;
            LocalMatrix = localMatrix;
            UpdateWorldMatrix(ref worldMatrix, false);
            Assert.AreEqual(localMatrix.M11, children.WorldMatrix.M11);

            InvalidateArrange();
            Arrange(Vector3.Zero, false);

            worldMatrix.M11 = 5;
            UpdateWorldMatrix(ref worldMatrix, false);
            Assert.AreEqual(worldMatrix.M11, children.WorldMatrix.M11);
        }
    }
}
