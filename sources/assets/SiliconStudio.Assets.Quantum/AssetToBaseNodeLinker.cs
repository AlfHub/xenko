using System;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum
{
    /// <summary>
    /// A <see cref="GraphNodeLinker"/> that can link nodes of an asset to the corresponding nodes in their base.
    /// </summary>
    /// <remarks>This method will invoke <see cref="AssetPropertyGraph.FindTarget(IContentNode, IContentNode)"/> when linking, to allow custom links for cases such as <see cref="AssetComposite"/>.</remarks>
    public class AssetToBaseNodeLinker : GraphNodeLinker
    {
        private readonly AssetPropertyGraph propertyGraph;

        public AssetToBaseNodeLinker(AssetPropertyGraph propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        public Func<IMemberNode, IContentNode, bool> ShouldVisit { get; set; }

        protected override bool ShouldVisitSourceNode(IMemberNode memberContent, IContentNode targetNode)
        {
            return (ShouldVisit?.Invoke(memberContent, targetNode) ?? true) && base.ShouldVisitSourceNode(memberContent, targetNode);
        }

        protected override IContentNode FindTarget(IContentNode sourceNode)
        {
            var defaultTarget = base.FindTarget(sourceNode);
            return propertyGraph.FindTarget(sourceNode, defaultTarget);
        }

        protected override ObjectReference FindTargetReference(IContentNode sourceNode, IContentNode targetNode, ObjectReference sourceReference)
        {
            // Not identifiable - default applies
            if (sourceReference.Index.IsEmpty || sourceReference.ObjectValue == null)
                return base.FindTargetReference(sourceNode, targetNode, sourceReference);

            // Special case for objects that are identifiable: the object must be linked to the base only if it has the same id
            var sourceAssetNode = (AssetObjectNode)sourceNode;
            var targetAssetNode = (AssetObjectNode)targetNode;
            if (!CollectionItemIdHelper.HasCollectionItemIds(sourceAssetNode.Retrieve()))
                return null;

            // Enumerable reference: we look for an object with the same id
            var targetReference = targetAssetNode.ItemReferences;
            var sourceIds = CollectionItemIdHelper.GetCollectionItemIds(sourceNode.Retrieve());
            var targetIds = CollectionItemIdHelper.GetCollectionItemIds(targetNode.Retrieve());
            var itemId = sourceIds[sourceReference.Index.Value];
            var targetKey = targetIds.GetKey(itemId);
            return targetReference.FirstOrDefault(x => Equals(x.Index.Value, targetKey));
        }
    }
}
