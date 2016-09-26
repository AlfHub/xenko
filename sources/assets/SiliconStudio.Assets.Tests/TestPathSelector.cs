﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets.Selectors;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestPathSelector
    {
        public bool TestSingleUrl(PathSelector pathSelector, string asset)
        {
            var assetIndexMap = new ObjectDatabaseContentIndexMap();
            assetIndexMap[asset] = ObjectId.New();

            return pathSelector.Select(null, assetIndexMap).Count() == 1;
        }

        private void TestPatterns(PathSelector pathSelector, string pattern, bool shouldMatch, bool allowPrefix, bool allowSuffix)
        {
            if (shouldMatch)
            {
                Assert.IsTrue(TestSingleUrl(pathSelector, pattern));
                Assert.IsTrue(TestSingleUrl(pathSelector, "a/" + pattern));
                Assert.IsTrue(TestSingleUrl(pathSelector, pattern + "/b"));
                Assert.IsTrue(TestSingleUrl(pathSelector, "a/" + pattern + "/b"));
            }
            else
            {
                Assert.IsFalse(TestSingleUrl(pathSelector, pattern));
                Assert.IsFalse(TestSingleUrl(pathSelector, "a/" + pattern));
                Assert.IsFalse(TestSingleUrl(pathSelector, pattern + "/b"));
                Assert.IsFalse(TestSingleUrl(pathSelector, "a/" + pattern + "/b"));
            }

            if (allowSuffix)
            {
                Assert.IsFalse(TestSingleUrl(pathSelector, pattern + "A"));
                Assert.IsFalse(TestSingleUrl(pathSelector, "a/" + pattern + "A"));
                Assert.IsFalse(TestSingleUrl(pathSelector, pattern + "A/b"));
                Assert.IsFalse(TestSingleUrl(pathSelector, "a/" + pattern + "A/b"));
            }

            if (allowPrefix)
            {
                Assert.IsFalse(TestSingleUrl(pathSelector, "A" + pattern));
                Assert.IsFalse(TestSingleUrl(pathSelector, "a/A" + pattern));
                Assert.IsFalse(TestSingleUrl(pathSelector, "A" + pattern + "/b"));
                Assert.IsFalse(TestSingleUrl(pathSelector, "a/A" + pattern + "/b"));
            }
        }

        [Test]
        public void TestSimple()
        {
            var pathSelector = new PathSelector();
            pathSelector.Paths.Add("simple");

            TestPatterns(pathSelector, "simple", true, true, true);
        }

        [Test]
        public void TestDouble()
        {
            var pathSelector = new PathSelector();
            pathSelector.Paths.Add("ab/cd");

            TestPatterns(pathSelector, "ab/cd", true, true, true);
        }

        [Test]
        public void TestStartEnd()
        {
            var pathSelector = new PathSelector();
            pathSelector.Paths.Add("/ab/cd");

            Assert.IsTrue(TestSingleUrl(pathSelector, "ab/cd"));
            Assert.IsTrue(TestSingleUrl(pathSelector, "ab/cd/de"));
            Assert.IsFalse(TestSingleUrl(pathSelector, "test/ab/cd"));

            pathSelector.Paths.Add("ab/cd/");
            Assert.IsFalse(TestSingleUrl(pathSelector, "xx/ab/cd"));
            Assert.IsTrue(TestSingleUrl(pathSelector, "xx/ab/cd/"));
            Assert.IsTrue(TestSingleUrl(pathSelector, "xx/ab/cd/de"));
        }

        [Test]
        public void TestEscape()
        {
            var pathSelector = new PathSelector();
            pathSelector.Paths.Add(@"\?\?");

            Assert.IsTrue(TestSingleUrl(pathSelector, "??"));
            Assert.IsFalse(TestSingleUrl(pathSelector, "ab"));
        }

        [Test]
        public void TestWildcard()
        {
            var pathSelector = new PathSelector();
            pathSelector.Paths.Add("*.test");

            TestPatterns(pathSelector, "a.test", true, false, true);
            TestPatterns(pathSelector, "a.test2", false, false, true);

            pathSelector.Paths[0] = "???.test";
            TestPatterns(pathSelector, "abc.test", true, false, true);
            TestPatterns(pathSelector, "ab.test", false, false, true);

            pathSelector.Paths[0] = "test.*";
            TestPatterns(pathSelector, "test.a", true, true, false);
            TestPatterns(pathSelector, "test2.a", false, true, false);
        }
    }
}