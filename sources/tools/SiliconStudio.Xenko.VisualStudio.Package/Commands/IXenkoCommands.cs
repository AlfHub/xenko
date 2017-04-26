// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using EnvDTE80;

using NShader;

namespace SiliconStudio.Xenko.VisualStudio.Commands
{
    /// <summary>
    /// Describes xenko commands accessed by VS Package to current xenko package (so that VSPackage doesn't depend on Xenko assemblies).
    /// </summary>
    /// <remarks>
    /// WARNING: Modifying this contract or any of it's dependencies will break backwards compatibility!
    /// Introduce a new contract instead (e.g. IXenkoCommands2).
    /// </remarks>
    public interface IXenkoCommands
    {
        /// <summary>
        /// Initialize parsing (this method can be called from a separate thread).
        /// </summary>
        void Initialize(string xenkoSdkDir);

        /// <summary>
        /// Test whether we should reload these commands (assemblies changed)
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool ShouldReload();

        void StartRemoteBuildLogServer(IBuildMonitorCallback buildMonitorCallback, string logPipeUrl);

        byte[] GenerateShaderKeys(string inputFileName, string inputFileContent);

        RawShaderNavigationResult AnalyzeAndGoToDefinition(string sourceCode, RawSourceSpan span);
    }

    public interface IBuildMonitorCallback
    {
        void Message(string type, string module, string text);
    }
}
