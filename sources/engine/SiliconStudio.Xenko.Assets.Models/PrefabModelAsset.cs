// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Models
{
    /// <summary>
    /// A model asset that is generated from a prefab, combining and merging meshes by materials and layout.
    /// </summary>
    [DataContract("PrefabModelAsset")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Model))]
    [Display((int)AssetDisplayPriority.Models + 60, "Prefab model")]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "0.0.0")]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public sealed class PrefabModelAsset : Asset, IModelAsset
    {
        private const string CurrentVersion = "2.0.0.0";

        /// <summary>
        /// The default file extension used by the <see cref="ProceduralModelAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkprefabmodel";

        /// <inheritdoc/>
        [DataMemberIgnore] // materials are not exposed in prefab models
        public List<ModelMaterial> Materials { get; } = new List<ModelMaterial>();

        [DataMember]
        public Prefab Prefab { get; set; }
    }
}
