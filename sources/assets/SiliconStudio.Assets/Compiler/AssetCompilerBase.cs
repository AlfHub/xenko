﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using SiliconStudio.Assets.Tracking;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Base implementation for <see cref="IAssetCompiler"/> suitable to compile a single type of <see cref="Asset"/>.
    /// </summary>
    /// <typeparam name="T">Type of the asset</typeparam>
    public abstract class AssetCompilerBase<T> : IAssetCompiler where T : Asset
    {
        public AssetCompilerResult Compile(CompilerContext context, AssetItem assetItem)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (assetItem == null) throw new ArgumentNullException(nameof(assetItem));

            var result = new AssetCompilerResult(GetType().Name)
            {
                BuildSteps = new AssetBuildStep(assetItem)
            };

            // Only use the path to the asset without its extension
            var fullPath = assetItem.FullPath;
            if (!fullPath.IsAbsolute)
            {
                throw new InvalidOperationException("assetItem must be an absolute path");
            }

            // Try to compile only if we're sure that the sources exist.
            if (EnsureSourcesExist(result, assetItem))
            {
                Compile((AssetCompilerContext)context, assetItem, (T)assetItem.Asset, result);
            }

            return result;
        }

        /// <summary>
        /// Compiles the asset from the specified package.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assetItem">The asset item being compiled</param>
        /// <param name="asset">The asset.</param>
        /// <param name="result">The result where the commands and logs should be output.</param>
        protected abstract void Compile(AssetCompilerContext context, AssetItem assetItem, T asset, AssetCompilerResult result);

        /// <summary>
        /// Returns the absolute path on the disk of an <see cref="UFile"/> that is relative to the asset location.
        /// </summary>
        /// <param name="assetAbsolutePath">The absolute path of the asset on the disk.</param>
        /// <param name="relativePath">The path relative to the asset path that must be converted to an absolute path.</param>
        /// <returns>The absolute path on the disk of the <see cref="relativePath"/> argument.</returns>
        /// <exception cref="ArgumentException">The <see cref="relativePath"/> argument is a null or empty <see cref="UFile"/>.</exception>
        protected static UFile GetAbsolutePath(UFile assetAbsolutePath, UFile relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentException("The relativePath argument is null or empty");
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, relativePath);
            return assetSource;
        }

        /// <summary>
        /// Ensures that the sources of an <see cref="Asset"/> exist.
        /// </summary>
        /// <param name="result">The <see cref="AssetCompilerResult"/> in which to output log of potential errors.</param>
        /// <param name="assetItem">The asset to check.</param>
        /// <returns><c>true</c> if the source file exists, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Any of the argument is <c>null</c>.</exception>
        private static bool EnsureSourcesExist(AssetCompilerResult result, AssetItem assetItem)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (assetItem == null) throw new ArgumentNullException(nameof(assetItem));

            var collector = new SourceFilesCollector();
            var sourceMembers = collector.GetSourceMembers(assetItem.Asset);

            foreach (var member in sourceMembers)
            {
                if (string.IsNullOrEmpty(member.Value))
                {
                    result.Error($"Source is null for Asset [{assetItem}] in property [{member.Key}]");
                    return false;
                }

                // Get absolute path of asset source on disk
                var assetDirectory = assetItem.FullPath.GetParent();
                var assetSource = UPath.Combine(assetDirectory, member.Value);

                // Ensure the file exists
                if (!File.Exists(assetSource))
                {
                    result.Error($"Unable to find the source file '{assetSource}' for Asset [{assetItem}]");
                    return false;
                }
            }

            return true;
        }
    }
}
