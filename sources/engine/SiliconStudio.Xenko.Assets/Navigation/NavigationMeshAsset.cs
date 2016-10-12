﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    [DataContract("NavigationMeshAsset")]
    [AssetDescription(FileExtension)]
    [Display("Navigation Mesh")]
    [AssetCompiler(typeof(NavigationMeshAssetCompiler))]
    public class NavigationMeshAsset : Asset, IAssetCompileTimeDependencies
    {
        public const string FileExtension = ".xknavmesh";

        /// <summary>
        /// Scene that is used for building the navigation mesh
        /// </summary>
        [DataMember(1000)]
        public Scene Scene { get; set; }

        /// <summary>
        /// The bounding box used for the navigation mesh, ignored if <see cref="AutoGenerateBoundingBox"/> is set to true
        /// </summary>
        [DataMember(1200)]
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// Toggles automatic generation of bounding boxed, might not work well with infinite planes.
        /// If this is enabled, the <see cref="BoundingBox"/> property is ignored
        /// </summary>
        [DataMember(1205)]
        public bool AutoGenerateBoundingBox { get; set; }
        
        /// <summary>
        /// Collision filter that indicates which colliders are used in navmesh generation
        /// </summary>
        [DataMember(1500)]
        public CollisionFilterGroupFlags IncludedCollisionGroups { get; set; }

        /// <summary>
        /// Build settings used by Recast
        /// </summary>
        [DataMember(2000)]
        public NavigationMeshBuildSettings BuildSettings { get; set; }

        /// <summary>
        /// Settings for agents used with this navigationMesh
        /// Every entry corresponds with a layer, which is used by <see cref="NavigationComponent.NavigationMeshLayer"/> to select one from this list at runtime
        /// </summary>
        [DataMember(2010)] public List<NavigationAgentSettings> NavigationMeshAgentSettings = new List<NavigationAgentSettings>();

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = NavigationMeshAgentSettings?.ComputeHash() ?? 0;
                hashCode = (hashCode*397) ^ BoundingBox.GetHashCode();
                hashCode = (hashCode*397) ^ AutoGenerateBoundingBox.GetHashCode();
                hashCode = (hashCode*397) ^ (int)IncludedCollisionGroups;
                hashCode = (hashCode*397) ^ BuildSettings.GetHashCode();
                if(Scene != null)
                    hashCode = (hashCode*397) ^ Scene.Name.GetHashCode();
                return hashCode;
            }
        }

        public IEnumerable<IReference> EnumerateCompileTimeDependencies(PackageSession session)
        {
            if (Scene != null)
            {
                var reference = AttachedReferenceManager.GetAttachedReference(Scene);
                yield return new AssetReference<SceneAsset>(reference.Id, reference.Url);
            }
        }
    }
}