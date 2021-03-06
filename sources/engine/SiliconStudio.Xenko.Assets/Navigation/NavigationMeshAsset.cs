// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Navigation;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    [DataContract("NavigationMeshAsset")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(NavigationMesh))]
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "0.0.0")]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "2.0.0.0", typeof(EmptyAssetUpgrader))]
#else
    [AssetFormatVersion(XenkoConfig.PackageName, CurrentVersion, "2.0.0.0")]
#endif
    public partial class NavigationMeshAsset : Asset
    {
        private const string CurrentVersion = "2.0.0.0";

        public const string FileExtension = ".xknavmesh";

        /// <summary>
        /// Scene that is used for building the navigation mesh
        /// </summary>
        /// <userdoc>
        /// The scene this navigation mesh applies to
        /// </userdoc>
        [DataMember(10)]
        public Scene Scene { get; set; }

        /// <summary>
        /// Collision filter that indicates which colliders are used in navmesh generation
        /// </summary>
        /// <userdoc>
        /// Set which collision groups the navigation mesh uses.
        /// </userdoc>
        [DataMember(20)]
        public CollisionFilterGroupFlags IncludedCollisionGroups { get; set; }

        /// <summary>
        /// Build settings used by Recast
        /// </summary>
        /// <userdoc>
        /// Advanced settings for the navigation mesh
        /// </userdoc>
        [DataMember(30)]
        public NavigationMeshBuildSettings BuildSettings { get; set; }

        /// <summary>
        /// Groups that this navigation mesh should be built for
        /// </summary>
        /// <userdoc>
        /// The groups that use this navigation mesh
        /// </userdoc>
        [DataMember(40)]
        public List<Guid> SelectedGroups { get; } = new List<Guid>();
    }
}
