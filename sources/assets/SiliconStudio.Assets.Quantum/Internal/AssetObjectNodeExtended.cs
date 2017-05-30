// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Internal
{
    internal struct AssetObjectNodeExtended
    {
        [NotNull] private readonly IAssetObjectNodeInternal node;
        private readonly Dictionary<string, IGraphNode> contents;
        private readonly Dictionary<ItemId, OverrideType> itemOverrides;
        private readonly Dictionary<ItemId, OverrideType> keyOverrides;
        private readonly HashSet<ItemId> disconnectedDeletedIds;
        private CollectionItemIdentifiers collectionItemIdentifiers;
        private ItemId restoringId;

        public AssetObjectNodeExtended([NotNull] IAssetObjectNodeInternal node)
        {
            this.node = node;
            contents = new Dictionary<string, IGraphNode>();
            itemOverrides = new Dictionary<ItemId, OverrideType>();
            keyOverrides = new Dictionary<ItemId, OverrideType>();
            disconnectedDeletedIds = new HashSet<ItemId>();
            collectionItemIdentifiers = null;
            restoringId = ItemId.Empty;
            PropertyGraph = null;
            BaseNode = null;
            ResettingOverride = false;
        }

        public AssetPropertyGraph PropertyGraph { get; private set; }

        public IGraphNode BaseNode { get; private set; }

        internal bool ResettingOverride { get; set; }

        public void SetContent(string key, IGraphNode node)
        {
            contents[key] = node;
        }

        public IGraphNode GetContent(string key)
        {
            IGraphNode node;
            contents.TryGetValue(key, out node);
            return node;
        }

        /// <inheritdoc/>
        public void ResetOverrideRecursively(Index indexToReset)
        {
            OverrideItem(false, indexToReset);
            PropertyGraph.ResetAllOverridesRecursively(node, indexToReset);
        }

        public void OverrideItem(bool isOverridden, Index index)
        {
            node.NotifyOverrideChanging();
            var id = IndexToId(index);
            SetOverride(isOverridden ? OverrideType.New : OverrideType.Base, id, itemOverrides);
            node.NotifyOverrideChanged();
        }

        public void OverrideKey(bool isOverridden, Index index)
        {
            node.NotifyOverrideChanging();
            var id = IndexToId(index);
            SetOverride(isOverridden ? OverrideType.New : OverrideType.Base, id, keyOverrides);
            node.NotifyOverrideChanged();
        }

        public void OverrideDeletedItem(bool isOverridden, ItemId deletedId)
        {
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                node.NotifyOverrideChanging();
                SetOverride(isOverridden ? OverrideType.New : OverrideType.Base, deletedId, itemOverrides);
                if (isOverridden)
                {
                    ids.MarkAsDeleted(deletedId);
                    disconnectedDeletedIds.Remove(deletedId);
                }
                else
                {
                    ids.UnmarkAsDeleted(deletedId);
                }
                node.NotifyOverrideChanged();
            }
        }

        public void DisconnectOverriddenDeletedItem(ItemId deletedId)
        {
            disconnectedDeletedIds.Add(deletedId);
            OverrideDeletedItem(false, deletedId);
        }

        public bool IsItemDeleted(ItemId itemId)
        {
            if (disconnectedDeletedIds.Contains(itemId))
                return true;

            var collection = node.Retrieve();
            CollectionItemIdentifiers ids;
            if (!TryGetCollectionItemIds(collection, out ids))
                throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return ids.IsDeleted(itemId);
        }

        private bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds)
        {
            if (collectionItemIdentifiers != null)
            {
                itemIds = collectionItemIdentifiers;
                return true;
            }

            var result = CollectionItemIdHelper.TryGetCollectionItemIds(instance, out collectionItemIdentifiers);
            itemIds = collectionItemIdentifiers;
            return result;
        }

        public void Restore(object restoredItem, ItemId id)
        {
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
            }
            // Actually restore the item.
            node.Add(restoredItem);
        }

        public void Restore(object restoredItem, Index index, ItemId id)
        {
            restoringId = id;
            node.Add(restoredItem, index);
            restoringId = ItemId.Empty;
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
            }
        }

        public void RemoveAndDiscard(object item, Index itemIndex, ItemId id)
        {
            node.Remove(item, itemIndex);
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
            }
        }

        public OverrideType GetItemOverride(Index index)
        {
            var result = OverrideType.Base;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return result;
            return itemOverrides.TryGetValue(id, out result) ? result : OverrideType.Base;
        }

        public OverrideType GetKeyOverride(Index index)
        {
            var result = OverrideType.Base;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return result;
            return keyOverrides.TryGetValue(id, out result) ? result : OverrideType.Base;
        }

        public bool IsItemInherited(Index index)
        {
            return BaseNode != null && !IsItemOverridden(index);
        }

        public bool IsKeyInherited(Index index)
        {
            return BaseNode != null && !IsKeyOverridden(index);
        }

        public bool IsItemOverridden(Index index)
        {
            OverrideType result;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return false;
            return itemOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        public bool IsItemOverriddenDeleted(ItemId id)
        {
            OverrideType result;
            return IsItemDeleted(id) && itemOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        public bool IsKeyOverridden(Index index)
        {
            OverrideType result;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return false;
            return keyOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        public IEnumerable<Index> GetOverriddenItemIndices()
        {
            if (BaseNode == null)
                yield break;

            CollectionItemIdentifiers ids;
            var collection = node.Retrieve();
            if (!TryGetCollectionItemIds(collection, out ids))
                yield break;

            foreach (var flags in itemOverrides)
            {
                if ((flags.Value & OverrideType.New) == OverrideType.New)
                {
                    // If the override is a deleted item, there's no matching index to return.
                    if (ids.IsDeleted(flags.Key))
                        continue;

                    yield return IdToIndex(flags.Key);
                }
            }
        }

        public IEnumerable<Index> GetOverriddenKeyIndices()
        {
            if (BaseNode == null)
                yield break;

            CollectionItemIdentifiers ids;
            var collection = node.Retrieve();
            if (!TryGetCollectionItemIds(collection, out ids))
                yield break;

            foreach (var flags in keyOverrides)
            {
                if ((flags.Value & OverrideType.New) == OverrideType.New)
                {
                    // If the override is a deleted item, there's no matching index to return.
                    if (ids.IsDeleted(flags.Key))
                        continue;

                    yield return IdToIndex(flags.Key);
                }
            }
        }

        public bool HasId(ItemId id)
        {
            Index index;
            return TryIdToIndex(id, out index);
        }

        public Index IdToIndex(ItemId id)
        {
            Index index;
            if (!TryIdToIndex(id, out index)) throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return index;
        }

        public bool TryIdToIndex(ItemId id, out Index index)
        {
            if (id == ItemId.Empty)
            {
                index = Index.Empty;
                return true;
            }

            var collection = node.Retrieve();
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(collection, out ids))
            {
                index = new Index(ids.GetKey(id));
                return !index.IsEmpty;
            }
            index = Index.Empty;
            return false;

        }

        public ItemId IndexToId(Index index)
        {
            ItemId id;
            if (!TryIndexToId(index, out id)) throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return id;
        }

        public bool TryIndexToId(Index index, out ItemId id)
        {
            if (index == Index.Empty)
            {
                id = ItemId.Empty;
                return true;
            }

            var collection = node.Retrieve();
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(collection, out ids))
            {
                return ids.TryGet(index.Value, out id);
            }
            id = ItemId.Empty;
            return false;
        }

        internal void OnItemChanged(object sender, ItemChangeEventArgs e)
        {
            var value = node.Retrieve();

            if (!CollectionItemIdHelper.HasCollectionItemIds(value))
                return;

            // Make sure that we have item ids everywhere we're supposed to.
            AssetCollectionItemIdHelper.GenerateMissingItemIds(e.Collection.Retrieve());

            // Clear the cached item identifier collection.
            collectionItemIdentifiers = null;

            // Create new ids for collection items
            var baseNode = (AssetObjectNode)BaseNode;
            var removedId = ItemId.Empty;
            var isOverriding = baseNode != null && !PropertyGraph.UpdatingPropertyFromBase;
            var itemIds = CollectionItemIdHelper.GetCollectionItemIds(node.Retrieve());
            var collectionDescriptor = node.Descriptor as CollectionDescriptor;
            switch (e.ChangeType)
            {
                case ContentChangeType.CollectionUpdate:
                    break;
                case ContentChangeType.CollectionAdd:
                {
                    // Compute the id we will add for this item
                    var itemId = restoringId != ItemId.Empty ? restoringId : ItemId.New();
                    // Add the id to the proper location (insert or add)
                    if (collectionDescriptor != null)
                    {
                        if (e.Index == Index.Empty)
                            throw new InvalidOperationException("An item has been added to a collection that does not have a predictable Add. Consider using NonIdentifiableCollectionItemsAttribute on this collection.");

                        itemIds.Insert(e.Index.Int, itemId);
                    }
                    else
                    {
                        itemIds[e.Index.Value] = itemId;
                    }
                    break;
                }
                case ContentChangeType.CollectionRemove:
                {
                    var itemId = itemIds[e.Index.Value];
                    // update isOverriding, it should be true only if the item being removed exist in the base.
                    isOverriding = isOverriding && baseNode.HasId(itemId);
                    removedId = collectionDescriptor != null ? itemIds.DeleteAndShift(e.Index.Int, isOverriding) : itemIds.Delete(e.Index.Value, isOverriding);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }


            // Don't update override if propagation from base is disabled.
            if (PropertyGraph?.Container?.PropagateChangesFromBase == false)
                return;

            // Mark it as New if it does not come from the base
            if (!ResettingOverride)
            {
                if (e.ChangeType != ContentChangeType.CollectionRemove && isOverriding)
                {
                    // If it's an add or an updating, there is no scenario where we can be "un-overriding" without ResettingOverride, so we pass true.
                    OverrideItem(true, e.Index);
                }
                else if (e.ChangeType == ContentChangeType.CollectionRemove)
                {
                    // If it's a delete, it could be an item that was previously added as an override, and that should not be marked as "deleted-overridden", so we pass isOverriding
                    OverrideDeletedItem(isOverriding, removedId);
                }
            }
        }

        private static void SetOverride(OverrideType overrideType, ItemId id, Dictionary<ItemId, OverrideType> dictionary)
        {
            if (overrideType == OverrideType.Base)
            {
                dictionary.Remove(id);
            }
            else
            {
                dictionary[id] = overrideType;
            }
        }

        public void SetPropertyGraph(AssetPropertyGraph assetPropertyGraph)
        {
            if (assetPropertyGraph == null) throw new ArgumentNullException(nameof(assetPropertyGraph));
            PropertyGraph = assetPropertyGraph;
        }

        public void SetBaseContent(IGraphNode baseNode)
        {
            BaseNode = baseNode;
        }
    }
}
