// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using NUnit.Framework;

using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetReferenceCollection
    {
        [Test]
        public void TestCollectionAddRemove()
        {
            // TODO test to be modified
           /*

            var assets = new AssetItemCollection();

            // Check that null are not allowed
            Assert.Throws<ArgumentNullException>(() => assets.Add(null));

            // Check that null location are not allowed
            Assert.Throws<ArgumentNullException>(() => assets.Add(new AssetItem(null, null)));

            // Test Find
            var ref1 = new AssetItem("a/test.txt", null);
            assets.Add(ref1);

            var ref2 = new AssetItem("b/test.txt", null);
            assets.Add(ref2);

            var findRef1 = assets.Find("a/test");
            Assert.AreEqual(ref1, findRef1);

            // Test Remove
            assets.Remove(ref1);
            Assert.AreEqual(assets.Count, 1);

            // Change location after adding an asset reference
            //ref1.Location = "a/test2.txt";
            assets.Add(ref1);
            //ref1.Location = "a/test3.txt";

            findRef1 = assets.Find("a/test3");
            Assert.AreEqual(ref1, findRef1);
            Assert.AreEqual(assets.Count, 2);

            // Add a reference with the same name
            Assert.Throws<ArgumentException>(() => assets.Add(new AssetItem("a/test3.png", null)));
            Assert.AreEqual(assets.Count, 2);

            // Test clear
            assets.Clear();
            Assert.AreEqual(assets.Count, 0);
            * */
        }
    }
}
