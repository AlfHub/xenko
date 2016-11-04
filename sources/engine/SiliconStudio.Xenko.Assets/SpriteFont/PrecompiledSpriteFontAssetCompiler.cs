﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.TextureConverter;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public class PrecompiledSpriteFontAssetCompiler : AssetCompilerBase
    {
        private static readonly FontDataFactory FontDataFactory = new FontDataFactory();

        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (PrecompiledSpriteFontAsset)assetItem.Asset;
            result.BuildSteps = new AssetBuildStep(assetItem) { new PrecompiledSpriteFontCommand(targetUrlInStorage, asset) };
        }

        internal class PrecompiledSpriteFontCommand : AssetCommand<PrecompiledSpriteFontAsset>
        {
            public PrecompiledSpriteFontCommand(string url, PrecompiledSpriteFontAsset description)
                : base(url, description)
            {
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                yield return new ObjectUrl(UrlType.File, Parameters.FontDataFile);
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                using (var texTool = new TextureTool())
                using (var texImage = texTool.Load(Parameters.FontDataFile, Parameters.IsSrgb))
                {
                    //make sure we are RGBA and not BGRA
                    texTool.Convert(texImage, Parameters.IsSrgb ? PixelFormat.R8G8B8A8_UNorm_SRgb : PixelFormat.R8G8B8A8_UNorm);

                    var image = texTool.ConvertToXenkoImage(texImage);

                    Graphics.SpriteFont staticFont = FontDataFactory.NewStatic(
                        Parameters.Size,
                        Parameters.Glyphs,
                        new[] { image },
                        Parameters.BaseOffset,
                        Parameters.DefaultLineSpacing,
                        Parameters.Kernings,
                        Parameters.ExtraSpacing,
                        Parameters.ExtraLineSpacing,
                        Parameters.DefaultCharacter);

                    // save the data into the database
                    var assetManager = new ContentManager();
                    assetManager.Save(Url, staticFont);

                    image.Dispose();
                }

                return Task.FromResult(ResultStatus.Successful);
            }
        }
    }
}
