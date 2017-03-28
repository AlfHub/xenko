﻿using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.Quantum.Tests.Helpers;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    // TODO: this class should be rewritten to properly match the new design of dependent properties, which is using hard-link between nodes instead of path-based.
    [TestFixture]
    public class TestDependentProperties
    {
        private const string Title = nameof(Types.DependentPropertyContainer.Title);
        private const string Instance = nameof(Types.DependentPropertyContainer.Instance);
        private const string Name = nameof(Types.SimpleObject.Name);
        private const string Nam = nameof(Types.SimpleObject.Nam);

        private const string TestDataKey = "TestData";
        private const string UpdateCountKey = "UpdateCount";
        private static readonly PropertyKey<string> TestData = new PropertyKey<string>(TestDataKey, typeof(TestDependentProperties));
        private static readonly PropertyKey<int> UpdateCount = new PropertyKey<int>(UpdateCountKey, typeof(TestDependentProperties));

        private abstract class DependentPropertiesUpdater : NodePresenterUpdaterBase
        {
            private int count;
            protected abstract bool IsRecursive { get; }

            public override void UpdateNode(INodePresenter node)
            {
                if (node.Name == nameof(Types.DependentPropertyContainer.Title))
                {
                    var instance = (Types.DependentPropertyContainer)node.Root.Value;
                    node.AttachedProperties.Set(TestData, instance.Instance.Name);
                    node.AttachedProperties.Set(UpdateCount, count++);
                }
            }

            public override void FinalizeTree(INodePresenter root)
            {
                var node = root[Title];
                var dependencyNode = GetDependencyNode(node.Root);
                node.AddDependency(dependencyNode, IsRecursive);
            }

            protected abstract INodePresenter GetDependencyNode(INodePresenter rootNode);
        }

        private class SimpleDependentPropertiesUpdater : DependentPropertiesUpdater
        {
            protected override bool IsRecursive => false;

            protected override INodePresenter GetDependencyNode(INodePresenter rootNode)
            {
                return rootNode[Instance][Name];
            }
        }

        private class RecursiveDependentPropertiesUpdater : DependentPropertiesUpdater
        {
            protected override bool IsRecursive => true;

            protected override INodePresenter GetDependencyNode(INodePresenter rootNode)
            {
                return rootNode[Instance];
            }
        }

        [Test]
        public void TestSimpleDependency()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContainerContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.AvailableUpdaters.Add(new SimpleDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue2";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestSimpleDependencyChangeParent()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContainerContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.AvailableUpdaters.Add(new SimpleDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var instanceNode = viewModel.RootNode.GetChild(Instance);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.NodeValue = new Types.SimpleObject { Name = "NewValue" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.NodeValue = new Types.SimpleObject { Name = "NewValue2" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestRecursiveDependency()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContainerContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.AvailableUpdaters.Add(new RecursiveDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var instanceNode = viewModel.RootNode.GetChild(Instance);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.NodeValue = new Types.SimpleObject { Name = "NewValue" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.NodeValue = new Types.SimpleObject { Name = "NewValue2" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestRecursiveDependencyChangeChild()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContainerContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.AvailableUpdaters.Add(new RecursiveDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue2";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestRecursiveDependencyMixedChanges()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContainerContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.AvailableUpdaters.Add(new RecursiveDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var instanceNode = viewModel.RootNode.GetChild(Instance);

            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.NodeValue = new Types.SimpleObject { Name = "NewValue2" };
            nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue3";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue3", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(3, titleNode.AssociatedData[UpdateCountKey]);

            instanceNode.NodeValue = new Types.SimpleObject { Name = "NewValue4" };
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue4", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(4, titleNode.AssociatedData[UpdateCountKey]);
        }

        [Test]
        public void TestChangeDifferentPropertyWithSameStart()
        {
            var container = new Types.DependentPropertyContainer { Title = "Title", Instance = new Types.SimpleObject { Name = "Test" } };
            var testContext = new TestContainerContext();
            var instanceContext = testContext.CreateInstanceContext(container);
            testContext.GraphViewModelService.AvailableUpdaters.Add(new SimpleDependentPropertiesUpdater());
            var viewModel = instanceContext.CreateViewModel();
            var titleNode = viewModel.RootNode.GetChild(Title);
            var nameNode = viewModel.RootNode.GetChild(Instance).GetChild(Name);
            var namNode = viewModel.RootNode.GetChild(Instance).GetChild(Nam);

            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            namNode.NodeValue = "NewValue";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("Test", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(0, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue2";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            namNode.NodeValue = "NewValue3";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue2", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(1, titleNode.AssociatedData[UpdateCountKey]);

            nameNode.NodeValue = "NewValue4";
            Assert.AreEqual(true, titleNode.AssociatedData.ContainsKey(TestDataKey));
            Assert.AreEqual("NewValue4", titleNode.AssociatedData[TestDataKey]);
            Assert.AreEqual(2, titleNode.AssociatedData[UpdateCountKey]);
        }
    }
}
