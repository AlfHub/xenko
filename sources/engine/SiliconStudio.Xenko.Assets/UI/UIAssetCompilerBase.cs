﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Assets.UI
{
    public abstract class UIAssetCompilerBase<T> : AssetCompilerBase
        where T : UIAssetBase
    {
        protected sealed override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (T)assetItem.Asset;
            result.BuildSteps = new AssetBuildStep(assetItem) { Create(targetUrlInStorage, asset) };
        }

        protected abstract UIConvertCommand Create(string url, T parameters);

        protected abstract class UIConvertCommand : AssetCommand<T>
        {
            protected UIConvertCommand(string url, T parameters)
                : base(url, parameters)
            {
            }

            protected sealed override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var uiObject = Create(commandContext);
                assetManager.Save(Url, uiObject);

                return Task.FromResult(ResultStatus.Successful);
            }

            protected abstract ComponentBase Create(ICommandContext commandContext);
        }
    }
}
