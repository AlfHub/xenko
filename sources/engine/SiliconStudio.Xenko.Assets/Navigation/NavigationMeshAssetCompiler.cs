﻿// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Assets.Physics;
using SiliconStudio.Xenko.Navigation;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    class NavigationMeshAssetCompiler : AssetCompilerBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (NavigationMeshAsset)assetItem.Asset;
            result.BuildSteps = new ListBuildStep();
            
            // Add navigation mesh dependencies
            foreach (var dep in asset.EnumerateCompileTimeDependencies(assetItem.Package.Session))
            {
                var colliderAssetItem = assetItem.Package.Session.FindAsset(dep.Id);
                var colliderShapeAsset = colliderAssetItem.Asset as ColliderShapeAsset;
                if (colliderShapeAsset != null)
                {
                    // Compile the collider assets first
                    result.BuildSteps.Add(new AssetBuildStep(colliderAssetItem)
                    {
                        new ColliderShapeAssetCompiler.ColliderShapeCombineCommand(colliderAssetItem.Location, colliderShapeAsset, assetItem.Package)
                    });
                }
            }

            result.BuildSteps.Add(new WaitBuildStep());

            // Compile the navigation mesh itself
            result.BuildSteps.Add(new AssetBuildStep(assetItem)
            {
                new NavmeshBuildCommand(targetUrlInStorage, assetItem, asset, context)
            });

            result.ShouldWaitForPreviousBuilds = true;
        }

        private class NavmeshBuildCommand : AssetCommand<NavigationMeshAsset>
        {
            private readonly Package package;
            private readonly ContentManager contentManager = new ContentManager();
            private readonly Dictionary<string, PhysicsColliderShape> loadedColliderShapes = new Dictionary<string, PhysicsColliderShape>();

            private NavigationMesh oldNavigationMesh;

            private UFile assetUrl;
            private NavigationMeshAsset asset;

            private int sceneHash = 0;
            private SceneAsset clonedSceneAsset;
            private GameSettingsAsset gameSettingsAsset;
            private bool sceneCloned = false; // Used so that the scene is only cloned once when ComputeParameterHash or DoCommand is called

            // Automatically calculated bounding box
            private NavigationMeshBuildSettings buildSettings;
            private List<StaticColliderData> staticColliderDatas = new List<StaticColliderData>();
            private List<BoundingBox> boundingBoxes = new List<BoundingBox>();

            public NavmeshBuildCommand(string url, AssetItem assetItem, NavigationMeshAsset value, AssetCompilerContext context)
                : base(url, value)
            {
                gameSettingsAsset = context.GetGameSettingsAsset();
                asset = value;
                package = assetItem.Package;
                assetUrl = url;
            }

            protected override IEnumerable<ObjectUrl> GetInputFilesImpl()
            {
                foreach (var compileTimeDependency in asset.EnumerateCompileTimeDependencies(package.Session))
                {
                    yield return new ObjectUrl(UrlType.ContentLink, compileTimeDependency.Location);
                }

                // TODO: Fix dependency on game settings
            }

            protected override void ComputeParameterHash(BinarySerializationWriter writer)
            {
                base.ComputeParameterHash(writer);

                EnsureClonedSceneAndHash();
                writer.Write(sceneHash);
                writer.Write(asset.SelectedGroups);
                
                var navigationSettings = gameSettingsAsset.GetOrCreate<NavigationSettings>();
                writer.Write(navigationSettings.Groups);
            }
            
            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var intermediateDataId = ComputeAssetIntermediateDataId();

                oldNavigationMesh = LoadIntermediateData(intermediateDataId);
                var navigationMeshBuilder = new NavigationMeshBuilder(oldNavigationMesh);
                navigationMeshBuilder.Logger = commandContext.Logger;

                foreach (var colliderData in staticColliderDatas)
                    navigationMeshBuilder.Add(colliderData);
                
                var navigationSettings = gameSettingsAsset.GetOrCreate<NavigationSettings>();
                var groupsLookup = navigationSettings.Groups.ToDictionary(x => x.Id, x => x);

                var groups = new List<NavigationMeshGroup>();
                // Resolve groups
                foreach (var groupId in asset.SelectedGroups)
                {
                    NavigationMeshGroup group;
                    if (groupsLookup.TryGetValue(groupId, out group))
                    {
                        groups.Add(group);
                    }
                    else
                    {
                        commandContext.Logger.Warning($"Group not defined in game settings {{{groupId}}}");
                    }
                }

                var result = navigationMeshBuilder.Build(asset.BuildSettings, groups, asset.IncludedCollisionGroups, boundingBoxes, CancellationToken.None);
                
                // Unload loaded collider shapes
                foreach (var pair in loadedColliderShapes)
                {
                    contentManager.Unload(pair.Key);
                }

                if (!result.Success)
                    return Task.FromResult(ResultStatus.Failed);

                // Save complete navigation mesh + intermediate data to cache
                SaveIntermediateData(intermediateDataId, result.NavigationMesh);

                // Clear intermediate data and save to content database
                result.NavigationMesh.Cache = null;
                contentManager.Save(assetUrl, result.NavigationMesh);

                return Task.FromResult(ResultStatus.Successful);
            }

            /// <summary>
            /// Computes a unique Id for this asset used to store intermediate / build cache data
            /// </summary>
            /// <returns>The object id for asset intermediate data</returns>
            private ObjectId ComputeAssetIntermediateDataId()
            {
                var stream = new DigestStream(Stream.Null);
                var writer = new BinarySerializationWriter(stream);
                writer.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
                writer.Write(CommandCacheVersion);

                // Write binary format version
                writer.Write(DataSerializer.BinaryFormatVersion);

                // Compute assembly hash
                ComputeAssemblyHash(writer);

                // Write asset Id
                writer.Write(asset.Id);

                return stream.CurrentHash;
            }

            /// <summary>
            /// Loads intermediate data used for building a navigation mesh
            /// </summary>
            /// <param name="objectId">The unique Id for this data in the object database</param>
            /// <returns>The found cached build or null if there is no previous build</returns>
            private NavigationMesh LoadIntermediateData(ObjectId objectId)
            {
                try
                {
                    var objectDatabase = ContentManager.FileProvider.ObjectDatabase;
                    using (var stream = objectDatabase.OpenStream(objectId))
                    {
                        var reader = new BinarySerializationReader(stream);
                        NavigationMesh result = new NavigationMesh();
                        reader.Serialize(ref result, ArchiveMode.Deserialize);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

            /// <summary>
            /// Saves intermediate data used for building a navigation mesh
            /// </summary>
            /// <param name="objectId">The unique Id for this data in the object database</param>
            /// <param name="build">The build data to save</param>
            private void SaveIntermediateData(ObjectId objectId, NavigationMesh build)
            {
                var objectDatabase = ContentManager.FileProvider.ObjectDatabase;
                using (var stream = objectDatabase.OpenStream(objectId, VirtualFileMode.Create, VirtualFileAccess.Write))
                {
                    var writer = new BinarySerializationWriter(stream);
                    writer.Serialize(ref build, ArchiveMode.Serialize);
                    writer.Flush();
                }
            }

            private void EnsureClonedSceneAndHash()
            {
                if (!sceneCloned)
                {
                    // Hash relevant scene objects
                    if (asset.Scene != null)
                    {
                        string sceneUrl = AttachedReferenceManager.GetUrl(asset.Scene);
                        var sceneAsset = (SceneAsset)package.Session.FindAsset(sceneUrl)?.Asset;

                        // Clone scene asset because we update the world transformation matrices
                        clonedSceneAsset = (SceneAsset)AssetCloner.Clone(sceneAsset);

                        // Turn the entire entity hierarchy into a single list
                        var sceneEntities = clonedSceneAsset.Hierarchy.Parts.Select(x => x.Entity).ToList();

                        sceneHash = 0;
                        foreach (var entity in sceneEntities)
                        {
                            // Collect bounding box entities
                            NavigationBoundingBoxComponent boundingBoxComponent = entity.Get<NavigationBoundingBoxComponent>();
                            // Collect static collider entities
                            StaticColliderComponent colliderComponent = entity.Get<StaticColliderComponent>();

                            if (boundingBoxComponent == null && colliderComponent == null)
                                continue;
                            
                            // Update world transform
                            entity.Transform.UpdateWorldMatrix();

                            if (boundingBoxComponent != null)
                            {
                                Vector3 scale;
                                Quaternion rotation;
                                Vector3 translation;
                                boundingBoxComponent.Entity.Transform.WorldMatrix.Decompose(out scale, out rotation, out translation);
                                var boundingBox = new BoundingBox(translation - boundingBoxComponent.Size * scale, translation + boundingBoxComponent.Size * scale);
                                boundingBoxes.Add(boundingBox);

                                // Hash collider for ComputeParameterHash
                                sceneHash = (sceneHash * 397) ^ boundingBox.GetHashCode();
                            }

                            if (colliderComponent != null)
                            {
                                staticColliderDatas.Add(new StaticColliderData
                                {
                                    Component = colliderComponent
                                });

                                if (colliderComponent.Enabled && !colliderComponent.IsTrigger && ((int)asset.IncludedCollisionGroups & (int)colliderComponent.CollisionGroup) != 0)
                                {
                                    // Load collider shape assets since the scene asset is being used, which does not have these loaded by default
                                    foreach (var desc in colliderComponent.ColliderShapes)
                                    {
                                        var shapeAssetDesc = desc as ColliderShapeAssetDesc;
                                        if (shapeAssetDesc?.Shape != null)
                                        {
                                            var assetReference = AttachedReferenceManager.GetAttachedReference(shapeAssetDesc.Shape);
                                            PhysicsColliderShape loadedColliderShape;
                                            if (!loadedColliderShapes.TryGetValue(assetReference.Url, out loadedColliderShape))
                                            {
                                                loadedColliderShape = contentManager.Load<PhysicsColliderShape>(assetReference.Url);
                                                loadedColliderShapes.Add(assetReference.Url, loadedColliderShape); // Store where we loaded the shapes from
                                            }
                                            shapeAssetDesc.Shape = loadedColliderShape;
                                        }
                                    }
                                }

                                // Hash collider for ComputeParameterHash
                                sceneHash = (sceneHash * 397) ^ Xenko.Navigation.NavigationMeshBuildUtils.HashEntityCollider(colliderComponent, asset.IncludedCollisionGroups);
                            }
                        }
                    }
                    sceneCloned = true;
                }
            }
        }
    }
}
