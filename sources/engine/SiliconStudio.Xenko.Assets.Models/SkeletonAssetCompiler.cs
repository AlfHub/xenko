﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Xenko.Assets.Models
{
    public class SkeletonAssetCompiler : AssetCompilerBase<SkeletonAsset>
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, SkeletonAsset asset, AssetCompilerResult result)
        {
            var assetSource = GetAbsolutePath(assetItem.FullPath, asset.Source);
            var extension = assetSource.GetFileExtension();
            var buildStep = new AssetBuildStep(assetItem);

            var importModelCommand = ImportModelCommand.Create(extension);
            if (importModelCommand == null)
            {
                result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", extension, assetSource);
                return;
            }

            importModelCommand.SourcePath = assetSource;
            importModelCommand.Location = assetItem.Location;
            importModelCommand.Mode = ImportModelCommand.ExportMode.Skeleton;
            importModelCommand.ScaleImport = asset.ScaleImport;
            importModelCommand.PivotPosition = asset.PivotPosition;
            importModelCommand.SkeletonNodesWithPreserveInfo = asset.NodesWithPreserveInfo;

            buildStep.Add(importModelCommand);

            result.BuildSteps = buildStep;
        }
    }
}
