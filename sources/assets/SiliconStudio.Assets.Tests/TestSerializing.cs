﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public partial class TestSerializing : TestBase
    {
        [TestFixtureSetUp]
        public void Initialize()
        {
            AssemblyRegistry.Register(typeof(TestSerializing).Assembly, AssemblyCommonCategories.Assets);
        }

        [Test]
        public void TestMyAssetObject()
        {
            var assetObject = new MyAsset();
            assetObject.Id = Guid.Empty;

            assetObject.Description = "This is a test";

            assetObject.AssetDirectory = new UDirectory("/test/dynamic/path/to/file/in/object/property");
            assetObject.AssetUrl = new UFile("/test/dynamic/path/to/file/in/object/property");

            //assetObject.Base = new AssetBase("/this/is/an/url/to/MyObject", null);

            assetObject.CustomReference2 = new AssetReference(Guid.Empty, "/this/is/an/url/to/MyCustomReference2");
            assetObject.CustomReferences.Add(new AssetReference(Guid.Empty, "/this/is/an/url/to/MyCustomReferenceItem1"));

            assetObject.SeqItems1.Add("value1");
            assetObject.SeqItems1.Add("value2");

            assetObject.SeqItems2.Add("value1");
            assetObject.SeqItems2.Add("value2");
            assetObject.SeqItems2.Add("value3");

            assetObject.SeqItems3.Add("value1");
            assetObject.SeqItems3.Add("value2");
            assetObject.SeqItems3.Add("value3");
            assetObject.SeqItems3.Add("value4");

            assetObject.SeqItems4.Add("value0");
            assetObject.SeqItems5.Add("value0");

            assetObject.MapItems1.Add("key1", 1);
            assetObject.MapItems1.Add("key2", 2);

            assetObject.MapItems2.Add("key1", 1);
            assetObject.MapItems2.Add("key2", 2);
            assetObject.MapItems2.Add("key3", 3);

            assetObject.MapItems3.Add("key1", 1);
            assetObject.MapItems3.Add("key2", 2);
            assetObject.MapItems3.Add("key3", 3);
            assetObject.MapItems3.Add("key4", 3);

            string testGenerated1 = DirectoryTestBase + @"TestSerializing\TestSerializing_TestMyAssetObject_Generated1.xkobj";
            string testGenerated2 = DirectoryTestBase + @"TestSerializing\TestSerializing_TestMyAssetObject_Generated2.xkobj";
            string referenceFilePath = DirectoryTestBase + @"TestSerializing\TestSerializing_TestMyAssetObject_Reference.xkobj";

            // First store the file on the disk and compare it to the reference
            GenerateAndCompare("Test Serialization 1", testGenerated1, referenceFilePath, assetObject);

            // Deserialize it
            var newAssetObject = AssetSerializer.Load<MyAsset>(testGenerated1).Asset;

            // Restore the deserialize version and compare it with the reference
            GenerateAndCompare("Test Serialization 2 - double check", testGenerated2, referenceFilePath, newAssetObject);
        }


        [Test]
        public void TestAssetItemCollection()
        {
            // Test serialization of asset items.

            var inputs = new List<AssetItem>();
            for (int i = 0; i < 10; i++)
            {
                var newAsset = new AssetObjectTest() { Name = "Test" + i };
                inputs.Add(new AssetItem("" + i, newAsset));
            }

            var asText = ToText(inputs);
            var outputs = FromText(asText);

            Assert.AreEqual(inputs.Select(item => item.Location), outputs.Select(item => item.Location));
            Assert.AreEqual(inputs.Select(item => item.Asset), outputs.Select(item => item.Asset));
        }

        private static string ToText(List<AssetItem> assetCollection)
        {
            var stream = new MemoryStream();
            AssetSerializer.Default.Save(stream, assetCollection);
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        private static List<AssetItem> FromText(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;

            bool aliasOccurred;
            Dictionary<ObjectPath, OverrideType> overrides;
            var assetItems = (List<AssetItem>)AssetSerializer.Default.Load(stream, null, null, out aliasOccurred, out overrides);
            if (aliasOccurred)
            {
                foreach (var assetItem in assetItems)
                {
                    assetItem.IsDirty = true;
                }
            }
            return assetItems;
        }

        static void Main()
        {
            new TestSerializing().TestMyAssetObject();
        }
    }
}
