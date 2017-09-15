// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum
{
    public class BoxedNode : ObjectNode
    {
        private GraphNodeBase boxedStructureOwner;
        private Index boxedStructureOwnerIndex;

        public BoxedNode([NotNull] INodeBuilder nodeBuilder, object value, Guid guid, [NotNull] ITypeDescriptor descriptor)
            : base(nodeBuilder, value, guid, descriptor, null)
        {
        }

        protected internal override void UpdateFromMember(object newValue, Index index)
        {
            Update(newValue, index, true);
        }

        internal void UpdateFromOwner(object newValue)
        {
            Update(newValue, Index.Empty, false);
        }

        internal void SetOwnerContent(IGraphNode ownerNode, Index index)
        {
            boxedStructureOwner = (GraphNodeBase)ownerNode;
            boxedStructureOwnerIndex = index;
        }

        private void Update(object newValue, Index index, bool updateStructureOwner)
        {
            if (!index.IsEmpty)
            {
                var collectionDescriptor = Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    collectionDescriptor.SetValue(Value, index.Int, newValue);
                }
                else if (dictionaryDescriptor != null)
                {
                    dictionaryDescriptor.SetValue(Value, index, newValue);
                }
                else
                    throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
            }
            else
            {
                SetValue(newValue);
                if (updateStructureOwner)
                {
                    boxedStructureOwner?.UpdateFromMember(newValue, boxedStructureOwnerIndex);
                }
            }
        }

        public override string ToString()
        {
            return $"{{Node: Boxed {Type.Name} = [{Value}]}}";
        }
    }
}
