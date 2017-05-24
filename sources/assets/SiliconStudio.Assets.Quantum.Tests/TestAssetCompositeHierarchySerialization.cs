using NUnit.Framework;
using SiliconStudio.Assets.Quantum.Tests.Helpers;
using SiliconStudio.Assets.Tests.Helpers;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetCompositeHierarchySerialization
    {
        const string SimpleHierarchyYaml = @"!MyAssetHierarchy
Id: 00000001-0001-0000-0100-000001000000
Tags: []
Hierarchy:
    RootParts:
        - ref!! 00000002-0002-0000-0200-000002000000
        - ref!! 00000001-0001-0000-0100-000001000000
    Parts:
        -   Part:
                Id: 00000001-0001-0000-0100-000001000000
                Children: []
        -   Part:
                Id: 00000002-0002-0000-0200-000002000000
                Children: []
";

        const string NestedHierarchyYaml = @"!MyAssetHierarchy
Id: 00000001-0001-0000-0100-000001000000
Tags: []
Hierarchy:
    RootParts:
        - ref!! 00000002-0002-0000-0200-000002000000
        - ref!! 00000001-0001-0000-0100-000001000000
    Parts:
        -   Part:
                Id: 00000001-0001-0000-0100-000001000000
                Children: []
        -   Part:
                Id: 00000002-0002-0000-0200-000002000000
                Children: []
        -   Part:
                Id: 00000003-0003-0000-0300-000003000000
                Children:
                    - ref!! 00000002-0002-0000-0200-000002000000
        -   Part:
                Id: 00000004-0004-0000-0400-000004000000
                Children:
                    - ref!! 00000001-0001-0000-0100-000001000000
";

        const string MissortedHierarchyYaml = @"!MyAssetHierarchy
Id: 00000001-0001-0000-0100-000001000000
Tags: []
Hierarchy:
    RootParts:
        - ref!! 00000002-0002-0000-0200-000002000000
        - ref!! 00000001-0001-0000-0100-000001000000
    Parts:
        -   Part:
                Id: 00000003-0003-0000-0300-000003000000
                Children:
                    - ref!! 00000002-0002-0000-0200-000002000000
        -   Part:
                Id: 00000002-0002-0000-0200-000002000000
                Children: []
        -   Part:
                Id: 00000001-0001-0000-0100-000001000000
                Children: []
        -   Part:
                Id: 00000004-0004-0000-0400-000004000000
                Children:
                    - ref!! 00000001-0001-0000-0100-000001000000
";

        [Test]
        public void TestSimpleDeserialization()
        {
            var asset = AssetFileSerializer.Load<Types.MyAssetHierarchy>(AssetTestContainer.ToStream(SimpleHierarchyYaml), $"MyAsset{Types.FileExtension}");
            Assert.AreEqual(2, asset.Asset.Hierarchy.RootParts.Count);
            Assert.AreEqual(GuidGenerator.Get(2), asset.Asset.Hierarchy.RootParts[0].Id);
            Assert.AreEqual(GuidGenerator.Get(1), asset.Asset.Hierarchy.RootParts[1].Id);
            Assert.AreEqual(2, asset.Asset.Hierarchy.Parts.Count);
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(1)));
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(2)));
        }

        [Test]
        public void TestSimpleSerialization()
        {
            //var asset = AssetFileSerializer.Load<Types.MyAssetHierarchy>(AssetTestContainer.ToStream(text), $"MyAsset{Types.FileExtension}");
            var asset = new Types.MyAssetHierarchy();
            asset.Hierarchy.Parts.Add(new Types.MyPartDesign { Part = new Types.MyPart { Id = GuidGenerator.Get(1) } });
            asset.Hierarchy.Parts.Add(new Types.MyPartDesign { Part = new Types.MyPart { Id = GuidGenerator.Get(2) } });
            asset.Hierarchy.RootParts.Add(asset.Hierarchy.Parts[GuidGenerator.Get(2)].Part);
            asset.Hierarchy.RootParts.Add(asset.Hierarchy.Parts[GuidGenerator.Get(1)].Part);
            var context = new AssetTestContainer<Types.MyAssetHierarchy, Types.MyAssetHierarchyPropertyGraph>(asset);
            context.BuildGraph();
            SerializationHelper.SerializeAndCompare(context.AssetItem, context.Graph, SimpleHierarchyYaml, false);
        }

        [Test]
        public void TestNestedDeserialization()
        {
            var asset = AssetFileSerializer.Load<Types.MyAssetHierarchy>(AssetTestContainer.ToStream(NestedHierarchyYaml), $"MyAsset{Types.FileExtension}");
            Assert.AreEqual(2, asset.Asset.Hierarchy.RootParts.Count);
            Assert.AreEqual(GuidGenerator.Get(2), asset.Asset.Hierarchy.RootParts[0].Id);
            Assert.AreEqual(GuidGenerator.Get(1), asset.Asset.Hierarchy.RootParts[1].Id);
            Assert.AreEqual(4, asset.Asset.Hierarchy.Parts.Count);
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(1)));
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(2)));
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(3)));
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(4)));
            Assert.AreEqual(1, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(3)].Part.Children.Count);
            Assert.AreEqual(asset.Asset.Hierarchy.Parts[GuidGenerator.Get(2)].Part, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(3)].Part.Children[0]);
            Assert.AreEqual(1, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(4)].Part.Children.Count);
            Assert.AreEqual(asset.Asset.Hierarchy.Parts[GuidGenerator.Get(1)].Part, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(4)].Part.Children[0]);
        }

        [Test]
        public void TestNestedSerialization()
        {
            //var asset = AssetFileSerializer.Load<Types.MyAssetHierarchy>(AssetTestContainer.ToStream(text), $"MyAsset{Types.FileExtension}");
            var asset = new Types.MyAssetHierarchy();
            asset.Hierarchy.Parts.Add(new Types.MyPartDesign { Part = new Types.MyPart { Id = GuidGenerator.Get(1) } });
            asset.Hierarchy.Parts.Add(new Types.MyPartDesign { Part = new Types.MyPart { Id = GuidGenerator.Get(2) } });
            asset.Hierarchy.Parts.Add(new Types.MyPartDesign { Part = new Types.MyPart { Id = GuidGenerator.Get(3), Children = { asset.Hierarchy.Parts[GuidGenerator.Get(2)].Part } } });
            asset.Hierarchy.Parts.Add(new Types.MyPartDesign { Part = new Types.MyPart { Id = GuidGenerator.Get(4), Children = { asset.Hierarchy.Parts[GuidGenerator.Get(1)].Part } } });
            asset.Hierarchy.RootParts.Add(asset.Hierarchy.Parts[GuidGenerator.Get(2)].Part);
            asset.Hierarchy.RootParts.Add(asset.Hierarchy.Parts[GuidGenerator.Get(1)].Part);
            var context = new AssetTestContainer<Types.MyAssetHierarchy, Types.MyAssetHierarchyPropertyGraph>(asset);
            context.BuildGraph();
            SerializationHelper.SerializeAndCompare(context.AssetItem, context.Graph, NestedHierarchyYaml, false);
        }

        [Test]
        public void TestMissortedPartsDeserialization()
        {
            var asset = AssetFileSerializer.Load<Types.MyAssetHierarchy>(AssetTestContainer.ToStream(MissortedHierarchyYaml), $"MyAsset{Types.FileExtension}");
            Assert.AreEqual(2, asset.Asset.Hierarchy.RootParts.Count);
            Assert.AreEqual(GuidGenerator.Get(2), asset.Asset.Hierarchy.RootParts[0].Id);
            Assert.AreEqual(GuidGenerator.Get(1), asset.Asset.Hierarchy.RootParts[1].Id);
            Assert.AreEqual(4, asset.Asset.Hierarchy.Parts.Count);
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(1)));
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(2)));
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(3)));
            Assert.True(asset.Asset.Hierarchy.Parts.ContainsKey(GuidGenerator.Get(4)));
            Assert.AreEqual(1, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(3)].Part.Children.Count);
            Assert.AreEqual(asset.Asset.Hierarchy.Parts[GuidGenerator.Get(2)].Part, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(3)].Part.Children[0]);
            Assert.AreEqual(1, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(4)].Part.Children.Count);
            Assert.AreEqual(asset.Asset.Hierarchy.Parts[GuidGenerator.Get(1)].Part, asset.Asset.Hierarchy.Parts[GuidGenerator.Get(4)].Part.Children[0]);
        }
    }
}