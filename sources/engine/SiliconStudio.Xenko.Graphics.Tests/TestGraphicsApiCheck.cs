// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
/* THIS CODE IS DISABLED, WE WILL HAVE TO CLEANUP ASSEMBLY DEPENDENCIES
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.IO;

using NUnit.Framework;

using SiliconStudio.Xenko.PublicApiCheck;

namespace SiliconStudio.Xenko.Graphics
{
    // CANNOT WORK INSIDE THE SAME SOLUTION. NEED TO RUN THIS OUTSIDE THE SOLUTION
    [TestFixture]
    [Description("Check public Graphics API consistency between Reference, Direct3D, OpenGL42, OpenGLES")]
    public class TestGraphicsApi
    {
        public const string Platform = "Windows";
        public const string Target = "Debug";

        private const string PathPattern = @"..\..\..\..\..\..\Build\{0}-{1}-{2}\{3}";

        private static readonly string RootPath = Environment.CurrentDirectory;

        private static readonly string ReferencePath = Path.Combine(RootPath, GraphicsPath("Null"));
        private static readonly string GraphicsDirect3DPath = Path.Combine(RootPath, GraphicsPath("Direct3D"));
        private static readonly string OpenGL4Path = Path.Combine(RootPath, GraphicsPath("OpenGL"));
        private static readonly string OpenGLESPath = Path.Combine(RootPath, GraphicsPath("OpenGLES"));

        private static string GraphicsPath(string api)
        {
            return string.Format(PathPattern, Platform, api, Target, "SiliconStudio.Xenko.Graphics.dll");
        }


        [Test]
        public void TestDirect3D()
        {
            Assert.That(ApiCheck.DiffAssemblyToString(ReferencePath, GraphicsDirect3DPath), Is.Null);
        }

        [Test]
        public void TestOpenGL42()
        {
            Assert.That(ApiCheck.DiffAssemblyToString(ReferencePath, OpenGL4Path), Is.Null);
        }

        [Test]
        public void TestOpenGLES()
        {
            Assert.That(ApiCheck.DiffAssemblyToString(ReferencePath, OpenGLESPath), Is.Null);
        }
    }
}
#endif
*/
