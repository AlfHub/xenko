using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A container for item identifiers and similar metadata that is associated to a collection or a dictionary.
    /// </summary>
    // TODO: Arrange the API of this class once all use cases have been implemented
    public class CollectionItemIdentifiers : IEnumerable<KeyValuePair<object, ItemId>>
    {
        private readonly Dictionary<object, ItemId> keyToIdMap = new Dictionary<object, ItemId>();

        private readonly HashSet<ItemId> deletedItems = new HashSet<ItemId>();

        public ItemId this[object key] { get { return keyToIdMap[key]; } set { Set(key, value); } }

        public IEnumerable<ItemId> DeletedItems => deletedItems;

        public int KeyCount => keyToIdMap.Count;

        public int DeletedCount => deletedItems.Count;

        public int Count => KeyCount + DeletedCount;

        public void Add(object key, ItemId id)
        {
            keyToIdMap.Add(key, id);
            if (deletedItems.Contains(id))
                UnmarkAsDeleted(id);
        }

        public void Set(object key, ItemId id)
        {
            keyToIdMap[key] = id;
            if (deletedItems.Contains(id))
                UnmarkAsDeleted(id);
        }


        public void Insert(int index, ItemId id)
        {
            for (var i = keyToIdMap.Count; i > index; --i)
            {
                keyToIdMap[i] = keyToIdMap[i-1];

            }
            keyToIdMap[index] = id;
            if (deletedItems.Contains(id))
                UnmarkAsDeleted(id);
        }

        public void Clear()
        {
            keyToIdMap.Clear();
            deletedItems.Clear();
        }

        public bool ContainsKey(object key)
        {
            return keyToIdMap.ContainsKey(key);
        }

        public bool TryGet(object key, out ItemId id)
        {
            return keyToIdMap.TryGetValue(key, out id);
        }

        public void Delete(object key, bool markAsDeleted = true)
        {
            var id = keyToIdMap[key];
            keyToIdMap.Remove(key);
            if (markAsDeleted)
            {
                MarkAsDeleted(id);
            }
        }

        public void DeleteAndShift(int index, bool markAsDeleted = true)
        {
            var id = keyToIdMap[index];
            for (var i = index + 1; i < keyToIdMap.Count; ++i)
            {
                keyToIdMap[i - 1] = keyToIdMap[i];
            }
            keyToIdMap.Remove(keyToIdMap.Count - 1);

            if (markAsDeleted)
            {
                MarkAsDeleted(id);
            }
        }

        public void MarkAsDeleted(ItemId id)
        {
            deletedItems.Add(id);
        }

        public void UnmarkAsDeleted(ItemId id)
        {
            deletedItems.Remove(id);
        }

        public void Validate(bool isList)
        {
            var ids = new HashSet<ItemId>(keyToIdMap.Values);
            if (ids.Count != keyToIdMap.Count)
                throw new InvalidOperationException("Two elements of the collection have the same id");

            foreach (var deleted in deletedItems)
                ids.Add(deleted);

            if (ids.Count != keyToIdMap.Count + deletedItems.Count)
                throw new InvalidOperationException("An id is both marked as deleted and associated to a key of the collection.");
        }

        /// <summary>
        /// Find the id that is present in the given <paramref name="baseIds"/> collection but absent from this instance.
        /// </summary>
        /// <param name="baseIds">The collection of id that contains one id missing in this collection.</param>
        /// <returns>The id present in <paramref name="baseIds"/> that is absent from this instance.</returns>
        /// <exception cref="InvalidOperationException">Multiple ids are missing in this instance.</exception>
        public ItemId FindMissingId(CollectionItemIdentifiers baseIds)
        {
            // Create an hashset with all ids, deleted or active, from this instance.
            var hashSet = new HashSet<ItemId>(deletedItems);
            foreach (var item in keyToIdMap)
            {
                hashSet.Add(item.Value);
            }

            var missingId = ItemId.Empty;
            foreach (var item in baseIds.keyToIdMap)
            {
                // Find an active id present in the baseIds that is not part of the hashset for this.
                if (!hashSet.Contains(item.Value))
                {
                    // TODO: if we have scenario where this is ok, I guess we can just return the first one.
                    if (missingId != ItemId.Empty)
                        throw new InvalidOperationException("Multiple ids are missing.");

                    missingId = item.Value;
                }
            }

            // Find an deleted instance id in the baseIds that is not part of the hashset for this.
            foreach (var item in baseIds.deletedItems)
            {
                if (!hashSet.Contains(item))
                {
                    // TODO: if we have scenario where this is ok, I guess we can just return the first one.
                    if (missingId != ItemId.Empty)
                        throw new InvalidOperationException("Multiple ids are missing.");

                    missingId = item;
                }
            }

            return missingId;
        }

        /// <summary>
        /// Find the id that is present in the given <paramref name="baseIds"/> collection but absent from this instance.
        /// </summary>
        /// <param name="baseIds">The collection of id that contains one id missing in this collection.</param>
        /// <returns>The id present in <paramref name="baseIds"/> that is absent from this instance.</returns>
        /// <exception cref="InvalidOperationException">Multiple ids are missing in this instance.</exception>
        public IEnumerable<ItemId> FindMissingIds(CollectionItemIdentifiers baseIds)
        {
            // Create an hashset with all ids, deleted or active, from this instance.
            var hashSet = new HashSet<ItemId>(deletedItems);
            foreach (var item in keyToIdMap)
            {
                hashSet.Add(item.Value);
            }

            foreach (var item in baseIds.keyToIdMap)
            {
                // Find an active id present in the baseIds that is not part of the hashset for this.
                if (!hashSet.Contains(item.Value))
                {
                    yield return item.Value;
                }
            }

            foreach (var item in baseIds.deletedItems)
            {
                // Find a deleted instance id in the baseIds that is not part of the hashset for this.
                if (!hashSet.Contains(item))
                {
                    yield return item;
                }
            }
        }

        public object GetKey(ItemId itemId)
        {
            // TODO: add indexing by guid to avoid O(n)
            return keyToIdMap.SingleOrDefault(x => x.Value == itemId).Key;
        }

        public void CloneInto(CollectionItemIdentifiers target, IReadOnlyDictionary<object, object> referenceTypeClonedKeys)
        {
            target.keyToIdMap.Clear();
            target.deletedItems.Clear();
            foreach (var key in keyToIdMap)
            {
                object clonedKey;
                if (key.Key.GetType().IsValueType || referenceTypeClonedKeys == null)
                {
                    target.Add(key.Key, key.Value);
                }
                else if (referenceTypeClonedKeys.TryGetValue(key.Key, out clonedKey))
                {
                    target.Add(clonedKey, key.Value);
                }
                else
                {
                    throw new KeyNotFoundException("Unable to find the non-value type key in the dictionary of cloned keys.");
                }
            }
        }

        public bool IsDeleted(ItemId itemId)
        {
            return DeletedItems.Contains(itemId);
        }

        public IEnumerator<KeyValuePair<object, ItemId>> GetEnumerator() => keyToIdMap.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
