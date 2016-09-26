﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Raw asset compiler.
    /// </summary>
    internal class RawAssetCompiler : AssetCompilerBase<RawAsset>
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, RawAsset asset, AssetCompilerResult result)
        {
            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetItem.FullPath, asset.Source);
            var importCommand = new ImportStreamCommand(assetItem.Location, assetSource) { DisableCompression = !asset.Compress };

            result.BuildSteps = new AssetBuildStep(assetItem) { importCommand };
        }
    }
}
