﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestPackageStore
    {
        [Test]
        public void TestDefault()
        {
            // Initialize a default package manager that will use the 
            var packageManager = PackageStore.Instance;

            // Build output is Bin\Windows\Tests\SiliconStudio.Assets.Tests, so need to go to parent 4 times
            var installationPath = (UDirectory)Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(TestPackageStore).Assembly.Location), @"..\..\..\.."));

            Assert.AreEqual(installationPath, packageManager.InstallationPath);

            var packageFileName = packageManager.GetPackageWithFileName(packageManager.DefaultPackageName);

            Assert.IsTrue(File.Exists(packageFileName), "Unable to find default package file [{0}]".ToFormat(packageFileName));
        }


        //[Test]
        //public void TestRemote()
        //{
        //    // Only work if the remote is correctly setup using the store.config nuget file
        //    var packageManager = new PackageStore();
        //    var installedPackages = packageManager.GetInstalledPackages().ToList();

        //    foreach (var packageMeta in packageManager.GetPackages())
        //    {
        //        Console.WriteLine("Package {0} {1}", packageMeta.Name, packageMeta.Version);
        //    }

        //}
         
    }
}