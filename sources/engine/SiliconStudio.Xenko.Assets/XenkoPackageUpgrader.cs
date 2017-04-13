﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Assets.Effect;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets
{
#if SILICONSTUDIO_XENKO_SUPPORT_BETA_UPGRADE
    [PackageUpgrader(XenkoConfig.PackageName, "1.10.0-alpha01", CurrentVersion)]
#else
    [PackageUpgrader(XenkoConfig.PackageName, "2.0.0.0", CurrentVersion)]
#endif
    public class XenkoPackageUpgrader : PackageUpgrader
    {
        public const string CurrentVersion = "2.0.0.0";

        public static readonly string DefaultGraphicsCompositorLevel9Url = "DefaultGraphicsCompositorLevel9";
        public static readonly string DefaultGraphicsCompositorLevel10Url = "DefaultGraphicsCompositorLevel10";

        public override bool Upgrade(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, IList<PackageLoadingAssetFile> assetFiles)
        {
            // Graphics Compositor asset
            if (dependency.Version.MinVersion < new PackageVersion("1.10.0-alpha02"))
            {
                // Find game settings (if there is none, it's not a game and nothing to do)
                var gameSettings = assetFiles.FirstOrDefault(x => x.AssetLocation == GameSettingsAsset.GameSettingsLocation);
                if (gameSettings != null)
                {
                    RunAssetUpgradersUntilVersion(log, dependentPackage, dependency.Name, gameSettings.Yield().ToList(), new PackageVersion("1.10.0-alpha02"));

                    using (var gameSettingsYaml = gameSettings.AsYamlAsset())
                    {
                        // Figure out graphics profile; default is Level_10_0 (which is same as GraphicsCompositor default)
                        var graphicsProfile = GraphicsProfile.Level_10_0;
                        try
                        {
                            foreach (var mapping in gameSettingsYaml.DynamicRootNode.Defaults)
                            {
                                if (mapping.Node.Tag == "!SiliconStudio.Xenko.Graphics.RenderingSettings,SiliconStudio.Xenko.Graphics")
                                {
                                    if (mapping.DefaultGraphicsProfile != null)
                                        Enum.TryParse((string)mapping.DefaultGraphicsProfile, out graphicsProfile);
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            // If something goes wrong, keep going with the default value
                        }

                        // Add graphics compositor asset by creating a derived asset of Compositing/DefaultGraphicsCompositor.xkgfxcomp
                        var graphicsCompositorUrl = graphicsProfile >= GraphicsProfile.Level_10_0 ? DefaultGraphicsCompositorLevel10Url : DefaultGraphicsCompositorLevel9Url;
                        var defaultGraphicsCompositor = dependencyPackage.Assets.Find(graphicsCompositorUrl);
                        if (defaultGraphicsCompositor == null)
                        {
                            log.Error($"Could not find graphics compositor in Xenko package at location [{graphicsCompositorUrl}]");
                            return false;
                        }

                        // Note: we create a derived asset without its content
                        // We don't use defaultGraphicsCompositor content because it might be a newer version that next upgrades might not understand.
                        // The override system will restore all the properties for us.
                        var graphicsCompositorAssetId = AssetId.New();
                        var graphicsCompositorAsset = new PackageLoadingAssetFile(dependentPackage, "GraphicsCompositor.xkgfxcomp", null)
                        {
                            AssetContent = System.Text.Encoding.UTF8.GetBytes($"!GraphicsCompositorAsset\r\nId: {graphicsCompositorAssetId}\r\nSerializedVersion: {{Xenko: 1.10.0-beta01}}\r\nArchetype: {defaultGraphicsCompositor.ToReference()}"),
                        };

                        assetFiles.Add(graphicsCompositorAsset);

                        // Update game settings to point to our newly created compositor
                        gameSettingsYaml.DynamicRootNode.GraphicsCompositor = new AssetReference(graphicsCompositorAssetId, graphicsCompositorAsset.AssetLocation).ToString();
                    }
                }

                // Delete EffectLogAsset
                foreach (var assetFile in assetFiles)
                {
                    if (assetFile.FilePath.GetFileName() == EffectLogAsset.DefaultFile)
                    {
                        assetFile.Deleted = true;
                    }
                }
            }


            if (dependency.Version.MinVersion < new PackageVersion("1.11.1.0"))
            {
                ConvertNormalMapsInvertY(assetFiles);
            }

            // Skybox/Background separation
            if (dependency.Version.MinVersion < new PackageVersion("1.11.1.1"))
            {
                SplitSkyboxLightingUpgrader upgrader = new SplitSkyboxLightingUpgrader();
                foreach (var skyboxAsset in assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xksky"))
                {
                    upgrader.ProcessSkybox(skyboxAsset);
                }
                foreach (var sceneAsset in assetFiles.Where(f => (f.FilePath.GetFileExtension() == ".xkscene") || (f.FilePath.GetFileExtension() == ".xkprefab")))
                {
                    using (var yaml = sceneAsset.AsYamlAsset())
                    {
                        upgrader.UpgradeAsset(yaml.DynamicRootNode);
                    }
                }
            }
            
            if (dependency.Version.MinVersion < new PackageVersion("1.11.1.2"))
            {
                var navigationMeshAssets = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xknavmesh");
                var scenes = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xkscene");
                UpgradeNavigationBoundingBox(navigationMeshAssets, scenes);

                // Upgrade game settings to have groups for navigation meshes
                var gameSettingsAsset = assetFiles.FirstOrDefault(x => x.AssetLocation == GameSettingsAsset.GameSettingsLocation);
                if (gameSettingsAsset != null)
                {
                    // Upgrade the game settings first to contain navigation mesh settings entry
                    RunAssetUpgradersUntilVersion(log, dependentPackage, dependency.Name, gameSettingsAsset.Yield().ToList(), new PackageVersion("1.11.1.2"));

                    UpgradeNavigationMeshGroups(navigationMeshAssets, gameSettingsAsset);
                }
            }

            return true;
        }

        private void RunAssetUpgradersUntilVersion(ILogger log, Package dependentPackage, string dependencyName, IList<PackageLoadingAssetFile> assetFiles, PackageVersion maxVersion)
        {
            foreach (var assetFile in assetFiles)
            {
                if (assetFile.Deleted)
                    continue;

                var context = new AssetMigrationContext(dependentPackage, assetFile.ToReference(), assetFile.FilePath.ToWindowsPath(), log);
                AssetMigration.MigrateAssetIfNeeded(context, assetFile, dependencyName, maxVersion);
            }
        }

        /// <inheritdoc/>
        public override bool UpgradeAfterAssetsLoaded(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage, PackageVersionRange dependencyVersionBeforeUpdate)
        {
            if (dependencyVersionBeforeUpdate.MinVersion < new PackageVersion("1.3.0-alpha02"))
            {
                // Add everything as root assets (since we don't know what the project was doing in the code before)
                foreach (var assetItem in dependentPackage.Assets)
                {
                    if (!AssetRegistry.IsAssetTypeAlwaysMarkAsRoot(assetItem.Asset.GetType()))
                        dependentPackage.RootAssets.Add(new AssetReference(assetItem.Id, assetItem.Location));
                }
            }

            if (dependencyVersionBeforeUpdate.MinVersion < new PackageVersion("1.6.0-beta"))
            {
                // Mark all assets dirty to force a resave
                foreach (var assetItem in dependentPackage.Assets)
                {
                    if (!(assetItem.Asset is SourceCodeAsset))
                    {
                        assetItem.IsDirty = true;
                    }
                }
            }

            return true;
        }

        public override bool UpgradeBeforeAssembliesLoaded(PackageSession session, ILogger log, Package dependentPackage, PackageDependency dependency, Package dependencyPackage)
        {
            return true;
        }
        private bool IsYamlAsset(PackageLoadingAssetFile assetFile)
        {
            // Determine if asset was Yaml or not
            var assetFileExtension = Path.GetExtension(assetFile.FilePath);
            assetFileExtension = assetFileExtension?.ToLowerInvariant();

            var serializer = AssetFileSerializer.FindSerializer(assetFileExtension);
            return serializer is YamlAssetSerializer;
        }

        private void UpgradeNavigationBoundingBox(IEnumerable<PackageLoadingAssetFile> navigationMeshes, IEnumerable<PackageLoadingAssetFile> scenes)
        {
            foreach (var navigationMesh in navigationMeshes)
            {
                using (var navmeshYamlAsset = navigationMesh.AsYamlAsset())
                {
                    var navmeshAsset = navmeshYamlAsset.DynamicRootNode;
                    var sceneId = (string)navmeshAsset.Scene;
                    var sceneName = sceneId.Split(':').Last();
                    var matchingScene = scenes.Where(x => x.AssetLocation == sceneName).FirstOrDefault();
                    if (matchingScene != null)
                    {
                        var boundingBox = navmeshAsset.BoundingBox;
                        var boundingBoxMin = new Vector3((float)boundingBox.Minimum.X, (float)boundingBox.Minimum.Y, (float)boundingBox.Minimum.Z);
                        var boundingBoxMax = new Vector3((float)boundingBox.Maximum.X, (float)boundingBox.Maximum.Y, (float)boundingBox.Maximum.Z);
                        var boundingBoxSize = (boundingBoxMax - boundingBoxMin) * 0.5f;
                        var boundingBoxCenter = boundingBoxSize + boundingBoxMin;
                            
                        using (var matchingSceneYamlAsset = matchingScene.AsYamlAsset())
                        {
                            var sceneAsset = matchingSceneYamlAsset.DynamicRootNode;
                            var parts = (DynamicYamlArray)sceneAsset.Hierarchy.Parts;
                            var rootParts = (DynamicYamlArray)sceneAsset.Hierarchy.RootPartIds;
                            dynamic newEntity = new DynamicYamlMapping(new YamlMappingNode());
                            newEntity.Id = Guid.NewGuid().ToString();
                            newEntity.Name = "Navigation bounding box";
                                
                            var components = new DynamicYamlMapping(new YamlMappingNode());

                            // Transform component
                            dynamic transformComponent = new DynamicYamlMapping(new YamlMappingNode());
                            transformComponent.Node.Tag = "!TransformComponent";
                            transformComponent.Id = Guid.NewGuid().ToString();
                            transformComponent.Position = new DynamicYamlMapping(new YamlMappingNode
                            {
                                { "X", $"{boundingBoxCenter.X}" }, { "Y", $"{boundingBoxCenter.Y}" }, { "Z", $"{boundingBoxCenter.Z}" }
                            });
                            transformComponent.Rotation = new DynamicYamlMapping(new YamlMappingNode
                            {
                                { "X", "0.0" }, { "Y", "0.0"}, { "Z", "0.0" }, { "W", "0.0" }
                            });
                            transformComponent.Scale = new DynamicYamlMapping(new YamlMappingNode
                            {
                                { "X", "1.0" }, { "Y", "1.0" }, { "Z", "1.0" }
                            });
                            transformComponent.Children = new DynamicYamlMapping(new YamlMappingNode());
                            components.AddChild(Guid.NewGuid().ToString("N"), transformComponent);

                            // Bounding box component
                            dynamic boxComponent = new DynamicYamlMapping(new YamlMappingNode());
                            boxComponent.Id = Guid.NewGuid().ToString();
                            boxComponent.Node.Tag = "!SiliconStudio.Xenko.Navigation.NavigationBoundingBoxComponent,SiliconStudio.Xenko.Navigation";
                            boxComponent.Size = new DynamicYamlMapping(new YamlMappingNode
                            {
                                { "X", $"{boundingBoxSize.X}" }, { "Y", $"{boundingBoxSize.Y}" }, { "Z", $"{boundingBoxSize.Z}" }
                            }); ;
                            components.AddChild(Guid.NewGuid().ToString("N"), boxComponent);

                            newEntity.Components = components;

                            dynamic part = new DynamicYamlMapping(new YamlMappingNode());
                            part.Entity = newEntity;
                            parts.Add(part);
                            rootParts.Add((string)newEntity.Id);

                            // Currently need to sort children by Id
                            List<YamlNode> partsList = (List<YamlNode>)parts.Node.Children;
                            var entityKey = new YamlScalarNode("Entity");
                            var idKey = new YamlScalarNode("Id");
                            partsList.Sort((x,y) =>
                            {
                                var entityA = (YamlMappingNode)((YamlMappingNode)x).Children[entityKey];
                                var entityB = (YamlMappingNode)((YamlMappingNode)y).Children[entityKey];
                                var guidA =  new Guid(((YamlScalarNode)entityA.Children[idKey]).Value);
                                var guidB = new Guid(((YamlScalarNode)entityB.Children[idKey]).Value);
                                return guidA.CompareTo(guidB);
                            });
                        }
                    }
                }
            }
        }

        private void UpgradeNavigationMeshGroups(IEnumerable<PackageLoadingAssetFile> navigationMeshAssets, PackageLoadingAssetFile gameSettingsAsset)
        {
            // Collect all unique groups from all navigation mesh assets
            Dictionary<ObjectId, YamlMappingNode> agentSettings = new Dictionary<ObjectId, YamlMappingNode>();
            foreach (var navigationMeshAsset in navigationMeshAssets)
            {
                using (var navigationMesh = navigationMeshAsset.AsYamlAsset())
                {
                    HashSet<ObjectId> selectedGroups = new HashSet<ObjectId>();
                    foreach (var setting in navigationMesh.DynamicRootNode.NavigationMeshAgentSettings)
                    {
                        var currentAgentSettings = setting.Value;
                        using (DigestStream digestStream = new DigestStream(Stream.Null))
                        {
                            BinarySerializationWriter writer = new BinarySerializationWriter(digestStream);
                            writer.Write((float)currentAgentSettings.Height);
                            writer.Write((float)currentAgentSettings.Radius);
                            writer.Write((float)currentAgentSettings.MaxClimb);
                            writer.Write((float)currentAgentSettings.MaxSlope.Radians);
                            if (!agentSettings.ContainsKey(digestStream.CurrentHash))
                                agentSettings.Add(digestStream.CurrentHash, currentAgentSettings.Node);
                            selectedGroups.Add(digestStream.CurrentHash);
                        }
                    }

                    // Replace agent settings with group reference on the navigation mesh
                    navigationMesh.DynamicRootNode.NavigationMeshAgentSettings = DynamicYamlEmpty.Default;
                    dynamic selectedGroupsMapping = navigationMesh.DynamicRootNode.SelectedGroups = new DynamicYamlMapping(new YamlMappingNode());
                    foreach (var selectedGroup in selectedGroups)
                    {
                        selectedGroupsMapping.AddChild(Guid.NewGuid().ToString("N"), selectedGroup.ToGuid().ToString("D"));
                    }
                }
            }

            // Add them to the game settings
            int groupIndex = 0;
            using (var gameSettings = gameSettingsAsset.AsYamlAsset())
            {
                var defaults = gameSettings.DynamicRootNode.Defaults;
                foreach (var setting in defaults)
                {
                    if (setting.Node.Tag == "!SiliconStudio.Xenko.Navigation.NavigationSettings,SiliconStudio.Xenko.Navigation")
                    {
                        var groups = setting.Groups as DynamicYamlArray;
                        foreach (var groupToAdd in agentSettings)
                        {
                            dynamic newGroup = new DynamicYamlMapping(new YamlMappingNode());
                            newGroup.Id = groupToAdd.Key.ToGuid().ToString("D");
                            newGroup.Name = $"Group {groupIndex++}";
                            newGroup.AgentSettings = groupToAdd.Value;
                            groups.Add(newGroup);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Splits skybox lighting functionality from background functionality
        /// </summary>
        private class SplitSkyboxLightingUpgrader
        {
            private readonly Dictionary<string, SkyboxAssetInfo> skyboxAssetInfos = new Dictionary<string, SkyboxAssetInfo>();
            
            public void UpgradeAsset(dynamic asset)
            {
                var parts = GetPartsArray(asset);
                foreach (dynamic part in parts)
                {
                    var entity = part.Entity;
                    var components = entity.Components;

                    List<ComponentInfo> skyboxInfos = new List<ComponentInfo>();
                    List<dynamic> skyboxKeys = new List<dynamic>();

                    // Find skybox components
                    foreach (dynamic component in components)
                    {
                        ComponentInfo componentInfo = GetComponentInfo(component);

                        if (componentInfo.Component.Node.Tag == "!SkyboxComponent")
                        {
                            skyboxInfos.Add(componentInfo);
                            skyboxKeys.Add(component);
                        }
                    }

                    if (skyboxInfos.Count == 0)
                        continue;

                    // Remove skybox light dependency on skybox component
                    foreach (var component in entity.Components)
                    {
                        ComponentInfo componentInfo = GetComponentInfo(component);
                        if (componentInfo.Component.Node.Tag == "!LightComponent")
                        {
                            var lightComponent = componentInfo.Component;
                            if (lightComponent.Type != null && lightComponent.Type.Node.Tag == "!LightSkybox")
                            {
                                // Use first skybox component
                                var skyboxInfo = skyboxInfos.First();
                                
                                // Combine light and skybox intensity into light intensity
                                var lightIntensity = lightComponent.Intensity;
                                var skyboxIntensity = skyboxInfo.Component.Intensity;
                                float intensity = (lightIntensity != null) ? lightIntensity : 1.0f;
                                intensity *= ((skyboxIntensity != null) ? (float)skyboxIntensity : 1.0f);
                                lightComponent.Intensity = intensity;

                                // Copy skybox assignment
                                lightComponent.Type["Skybox"] = (string)skyboxInfo.Component.Skybox;

                                // Check if this light is now referencing a removed skybox asset
                                string referenceId = ((string)skyboxInfo.Component.Skybox)?.Split('/').Last().Split(':').First();
                                if (referenceId == null || !skyboxAssetInfos.ContainsKey(referenceId) || skyboxAssetInfos[referenceId].Deleted)
                                {
                                    lightComponent.Type["Skybox"] = "null";
                                }

                                // 1 light per entity max.
                                break;
                            }
                        }
                    }

                    // Add background components
                    foreach (var skyboxInfo in skyboxInfos)
                    {
                        SkyboxAssetInfo skyboxAssetInfo;
                        if (skyboxInfo.Component.Skybox == null)
                            continue;

                        string referenceId = ((string)skyboxInfo.Component.Skybox).Split('/').Last().Split(':').First();
                        if (!skyboxAssetInfos.TryGetValue(referenceId, out skyboxAssetInfo))
                            continue;
                        
                        if (skyboxAssetInfo.IsBackground)
                        {
                            var backgroundComponentNode = new YamlMappingNode();
                            backgroundComponentNode.Tag = "!BackgroundComponent";
                            backgroundComponentNode.Add("Texture", skyboxAssetInfo.TextureReference);
                            if (skyboxInfo.Component.Intensity != null)
                                backgroundComponentNode.Add("Intensity", (string)skyboxInfo.Component.Intensity);
                            AddComponent(components, backgroundComponentNode, Guid.NewGuid());
                        }
                    }

                    // Remove skybox components
                    foreach (var skybox in skyboxKeys)
                    {
                        RemoveComponent(components, skybox);
                    }
                }
            }

            public void ProcessSkybox(PackageLoadingAssetFile skyboxAsset)
            {
                using (var skyboxYaml = skyboxAsset.AsYamlAsset())
                {
                    var root = skyboxYaml.DynamicRootNode;
                    var rootMapping = (DynamicYamlMapping)root;

                    string cubemapReference = "null";

                    // Insert cubmap into skybox root instead of in Model
                    if (root.Model != null)
                    {
                        if (root.Model.Node.Tag == "!SkyboxCubeMapModel")
                        {
                            cubemapReference = root.Model.CubeMap;
                        }
                        rootMapping.RemoveChild("Model");
                    }
                    rootMapping.AddChild("CubeMap", cubemapReference);
                    var splitReference = cubemapReference.Split('/'); // TODO
                    
                    // We will remove skyboxes that are only used as a background
                    if (root.Usage != null && (string)root.Usage == "Background")
                    {
                        skyboxAsset.Deleted = true;
                    }
                    
                    bool isBackground = root.Usage == null ||
                                        (string)root.Usage == "Background" ||
                                        (string)root.Usage == "LightingAndBackground";
                    skyboxAssetInfos.Add((string)root.Id, new SkyboxAssetInfo
                    {
                        TextureReference = splitReference.Last(),
                        IsBackground = isBackground,
                        Deleted = skyboxAsset.Deleted,
                    });
                }
            }

            private void AddComponent(dynamic componentsNode, YamlMappingNode node, Guid id)
            {
                try
                {
                    // New format (1.9)
                    DynamicYamlMapping mapping = (DynamicYamlMapping)componentsNode;
                    mapping.AddChild(new YamlScalarNode(Guid.NewGuid().ToString("N")), node);
                    node.Add("Id", id.ToString("D"));
                }
                catch (Exception)
                {
                    // Old format (<= 1.8)
                    DynamicYamlArray array = (DynamicYamlArray)componentsNode;
                    node.Add("~Id", id.ToString("D")); // TODO
                    array.Add(node);
                }
            }

            private void RemoveComponent(dynamic componentsNode, dynamic componentsEntry)
            {
                try
                {
                    // New format (1.9)
                    DynamicYamlMapping mapping = (DynamicYamlMapping)componentsNode;
                    mapping.RemoveChild(componentsEntry.Key);
                }
                catch (Exception)
                {
                    // Old format (<= 1.8)
                    DynamicYamlArray array = (DynamicYamlArray)componentsNode;
                    for (int i = 0; i < array.Count; i++)
                    {
                        if (componentsNode[i].Node == componentsEntry.Node)
                        {
                            array.RemoveAt(i);
                            return;
                        }
                    }
                }
            }

            private DynamicYamlArray GetPartsArray(dynamic asset)
            {
                var hierarchy = asset.Hierarchy;
                if (hierarchy.Parts != null)
                    return (DynamicYamlArray)hierarchy.Parts; // > 1.6.0
                return (DynamicYamlArray)hierarchy.Entities; // <= 1.6.0
            }
            
            private ComponentInfo GetComponentInfo(dynamic componentNode)
            {
                if(componentNode.Key != null && componentNode.Value != null)
                {
                    // New format (1.9)
                    return new ComponentInfo
                    {
                        Id = (string)componentNode.Key,
                        Component = componentNode.Value
                    };
                }
                else
                {
                    // Old format (<= 1.8)
                    return new ComponentInfo
                    {
                        Id = (string)componentNode["~Id"], // TODO
                        Component = componentNode
                    };
                }
            }

            private struct SkyboxAssetInfo
            {
                public string TextureReference;
                public bool IsBackground;
                public bool Deleted;
            }

            private struct ComponentInfo
            {
                public string Id;
                public dynamic Component;
            }
        }

        private void ConvertNormalMapsInvertY(IList<PackageLoadingAssetFile> assetFiles)
        {
            var materialAssets = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xkmat").ToList();
            var textureAssets = assetFiles.Where(f => f.FilePath.GetFileExtension() == ".xktex").ToList();

            foreach (var materialFile in materialAssets)
            {
                if (!IsYamlAsset(materialFile))
                    continue;

                // This upgrader will also mark every yaml asset as dirty. We want to re-save everything with the new serialization system
                using (var yamlAsset = materialFile.AsYamlAsset())
                {
                    dynamic asset = yamlAsset.DynamicRootNode;

                    var assetTag = asset.Node.Tag;
                    if (assetTag != "!MaterialAsset")
                        continue;

                    if (asset.Attributes.Surface == null)
                        continue;

                    var surface = asset.Attributes.Surface;
                    var materialTag = surface.Node.Tag;
                    if (materialTag != "!MaterialNormalMapFeature")
                        continue;

                    var invertY = (asset.Attributes.Surface.InvertY == null || asset.Attributes.Surface.InvertY == "true");
                    if (invertY)
                        continue; // This is the default value for normal map textures, so no need to change it

                    // TODO Find all referenced files
                    if (asset.Attributes.Surface.NormalMap == null || asset.Attributes.Surface.NormalMap.Node.Tag != "!ComputeTextureColor")
                        continue;

                    dynamic texture = asset.Attributes.Surface.NormalMap.Texture;
                    var textureId = (string)texture.Node.Value;

                    foreach (var textureFile in textureAssets)
                    {
                        if (!IsYamlAsset(textureFile))
                            continue;

                        using (var yamlAssetTex = textureFile.AsYamlAsset())
                        {
                            dynamic assetTex = yamlAssetTex.DynamicRootNode;

                            var assetTagTex = assetTex.Node.Tag;
                            if (assetTagTex != "!Texture")
                                continue;

                            var assetIdTex = (string)assetTex.Id;
                            if (!textureId.Contains(assetIdTex))
                                continue;

                            assetTex["InvertY"] = false;
                        }
                    }
                }
            }
        }
    }
}
