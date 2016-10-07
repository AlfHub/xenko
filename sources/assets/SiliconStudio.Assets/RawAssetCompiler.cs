﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Raw asset compiler.
    /// </summary>
    internal class RawAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (RawAsset)assetItem.Asset;

            // Get absolute path of asset source on disk
            var assetSource = GetAbsolutePath(assetItem, asset.Source);
            var importCommand = new ImportStreamCommand(targetUrlInStorage, assetSource) { DisableCompression = !asset.Compress };

            result.BuildSteps = new AssetBuildStep(assetItem) { importCommand };
        }
    }
}
