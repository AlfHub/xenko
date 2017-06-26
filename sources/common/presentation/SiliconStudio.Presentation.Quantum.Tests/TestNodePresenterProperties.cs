// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Presentation.Quantum.Tests.Helpers;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    [TestFixture]
    public class TestNodePresenterProperties
    {
        [DataContract]
        public class SimpleMember
        {
            public float FloatValue { get; set; }
        }

        [DataContract]
        public class SimpleMemberWithContract
        {
            [DataMember(10)]
            public float FloatValue { get; set; }
        }

        public class NestedMemberClass
        {
            [DataMember(20)]
            public SimpleMemberWithContract MemberClass { get; set; } = new SimpleMemberWithContract();
        }

        public class NestedReadonlyMemberClass
        {
            [DataMember(30)]
            public SimpleMemberWithContract MemberClass { get; } = new SimpleMemberWithContract();
        }

        public class ListMember
        {
            [DataMember(40)]
            public List<string> List { get; set; }
        }

        [Test]
        public void TestSimpleMember()
        {
            var instance = new SimpleMember { FloatValue = 1.0f };
            var context = BuildContext(instance);
            var root = context.Factory.CreateNodeHierarchy(context.RootNode, new GraphNodePath(context.RootNode));
            var member = root[nameof(SimpleMember.FloatValue)];
            Assert.AreEqual(0, member.Children.Count);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), member.DisplayName);
            Assert.AreEqual(Index.Empty, member.Index);
            Assert.False(member.IsEnumerable);
            Assert.False(member.IsReadOnly);
            Assert.True(member.IsVisible);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), member.Name);
            Assert.Null(member.Order);
            Assert.AreEqual(root, member.Parent);
            Assert.AreEqual(1.0f, member.Value);
        }

        [Test]
        public void TestSimpleMemberWithContract()
        {
            var instance = new SimpleMemberWithContract { FloatValue = 1.0f };
            var context = BuildContext(instance);
            var root = context.Factory.CreateNodeHierarchy(context.RootNode, new GraphNodePath(context.RootNode));
            var member = root[nameof(SimpleMember.FloatValue)];
            Assert.AreEqual(0, member.Children.Count);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), member.DisplayName);
            Assert.AreEqual(Index.Empty, member.Index);
            Assert.False(member.IsEnumerable);
            Assert.False(member.IsReadOnly);
            Assert.True(member.IsVisible);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), member.Name);
            Assert.AreEqual(10, member.Order);
            Assert.AreEqual(root, member.Parent);
            Assert.AreEqual(1.0f, member.Value);
        }

        [Test]
        public void TestNestedMember()
        {
            var instance = new NestedMemberClass { MemberClass = { FloatValue = 1.0f } };
            var context = BuildContext(instance);
            var root = context.Factory.CreateNodeHierarchy(context.RootNode, new GraphNodePath(context.RootNode));
            var member = root[nameof(NestedMemberClass.MemberClass)];
            Assert.AreEqual(1, member.Children.Count);
            Assert.AreEqual(nameof(NestedMemberClass.MemberClass), member.DisplayName);
            Assert.AreEqual(Index.Empty, member.Index);
            Assert.False(member.IsEnumerable);
            Assert.False(member.IsReadOnly);
            Assert.True(member.IsVisible);
            Assert.AreEqual(nameof(NestedMemberClass.MemberClass), member.Name);
            Assert.AreEqual(20, member.Order);
            Assert.AreEqual(root, member.Parent);
            Assert.AreEqual(instance.MemberClass, member.Value);
            var innerMember = member[nameof(SimpleMember.FloatValue)];
            Assert.AreEqual(0, innerMember.Children.Count);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), innerMember.DisplayName);
            Assert.AreEqual(Index.Empty, innerMember.Index);
            Assert.False(innerMember.IsEnumerable);
            Assert.False(innerMember.IsReadOnly);
            Assert.True(innerMember.IsVisible);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), innerMember.Name);
            Assert.AreEqual(10, innerMember.Order);
            Assert.AreEqual(member, innerMember.Parent);
            Assert.AreEqual(1.0f, innerMember.Value);
        }

        [Test]
        public void TestNestedReadOnlyMember()
        {
            var instance = new NestedReadonlyMemberClass { MemberClass = { FloatValue = 1.0f } };
            var context = BuildContext(instance);
            var root = context.Factory.CreateNodeHierarchy(context.RootNode, new GraphNodePath(context.RootNode));
            var member = root[nameof(NestedMemberClass.MemberClass)];
            Assert.AreEqual(1, member.Children.Count);
            Assert.AreEqual(nameof(NestedMemberClass.MemberClass), member.DisplayName);
            Assert.AreEqual(Index.Empty, member.Index);
            Assert.False(member.IsEnumerable);
            Assert.True(member.IsReadOnly);
            Assert.True(member.IsVisible);
            Assert.AreEqual(nameof(NestedMemberClass.MemberClass), member.Name);
            Assert.AreEqual(30, member.Order);
            Assert.AreEqual(root, member.Parent);
            Assert.AreEqual(instance.MemberClass, member.Value);
            var innerMember = member[nameof(SimpleMember.FloatValue)];
            Assert.AreEqual(0, innerMember.Children.Count);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), innerMember.DisplayName);
            Assert.AreEqual(Index.Empty, innerMember.Index);
            Assert.False(innerMember.IsEnumerable);
            Assert.False(innerMember.IsReadOnly);
            Assert.True(innerMember.IsVisible);
            Assert.AreEqual(nameof(SimpleMember.FloatValue), innerMember.Name);
            Assert.AreEqual(10, innerMember.Order);
            Assert.AreEqual(member, innerMember.Parent);
            Assert.AreEqual(1.0f, innerMember.Value);
        }

        [Test]
        public void TestListMember()
        {
            var instance = new ListMember { List = new List<string>() };
            var context = BuildContext(instance);
            var root = context.Factory.CreateNodeHierarchy(context.RootNode, new GraphNodePath(context.RootNode));
            var member = root[nameof(ListMember.List)];
            Assert.AreEqual(0, member.Children.Count);
            Assert.AreEqual(nameof(ListMember.List), member.DisplayName);
            Assert.AreEqual(Index.Empty, member.Index);
            Assert.True(member.IsEnumerable);
            Assert.False(member.IsReadOnly);
            Assert.True(member.IsVisible);
            Assert.AreEqual(nameof(ListMember.List), member.Name);
            Assert.AreEqual(40, member.Order);
            Assert.AreEqual(root, member.Parent);
            Assert.AreEqual(instance.List, member.Value);

            instance = new ListMember();
            context = BuildContext(instance);
            root = context.Factory.CreateNodeHierarchy(context.RootNode, new GraphNodePath(context.RootNode));
            member = root[nameof(ListMember.List)];
            Assert.AreEqual(0, member.Children.Count);
            Assert.AreEqual(nameof(ListMember.List), member.DisplayName);
            Assert.AreEqual(Index.Empty, member.Index);
            Assert.False(member.IsEnumerable);
            Assert.False(member.IsReadOnly);
            Assert.True(member.IsVisible);
            Assert.AreEqual(nameof(ListMember.List), member.Name);
            Assert.AreEqual(40, member.Order);
            Assert.AreEqual(root, member.Parent);
            Assert.AreEqual(instance.List, member.Value);
        }

        private static TestInstanceContext BuildContext(object instance)
        {
            var context = new TestContainerContext();
            return context.CreateInstanceContext(instance);
        }
    }
}
