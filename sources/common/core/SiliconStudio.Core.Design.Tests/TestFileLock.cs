﻿// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using NUnit.Framework;
using SiliconStudio.Core.Windows;

namespace SiliconStudio.Core.Design.Tests
{
    [TestFixture]
    public class TestFileLock
    {
        [Test]
        public void TestFilelockWait()
        {
            FileLock mutex;
            bool flag;

            // Creating lock in current directory.
            using (mutex = FileLock.Wait("something.lock"))
            {
            }

            // Explicitely creating lock in current directory.
            var dir = Directory.GetCurrentDirectory();
            using (mutex = FileLock.Wait(Path.Combine(dir, "something.lock")))
            {
            }

            // Creating lock in a directory that does not yet exist, it should throw an exception.
            flag = false;
            var guidDir = Path.Combine(dir, Guid.NewGuid().ToString());
            try
            {
                using (mutex = FileLock.Wait(Path.Combine(guidDir, "something.lock")))
                {
                    // This should never happen. So throw an exception and make sure it is not caught by our catch below.
                    flag = true;
                    Assert.IsTrue(false, "Cannot create a file lock if parent directory does not exist.");
                }
            }
            catch (Exception)
            {
                if (flag) throw;
            }

            // Create lock in a directory that exists.
            Directory.CreateDirectory(guidDir);
            using (mutex = FileLock.Wait(Path.Combine(dir, guidDir, "something.lock")))
            {
            }

            // Delete our temporary directory.
            Directory.Delete(guidDir, true);
        }
    }
}