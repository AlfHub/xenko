﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.BuildEngine;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Main interface for compiling an <see cref="Asset"/>.
    /// </summary>
    public interface IAssetCompiler
    {
        /// <summary>
        /// Compiles a list of assets from the specified package.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assetItem">The asset reference.</param>
        /// <returns>The result of the compilation.</returns>
        AssetCompilerResult Compile(CompilerContext context, AssetItem assetItem);

        IEnumerable<AssetBuildStep> GetBuildDependencies(CompilerContext context, AssetItem assetItem);
    }
}