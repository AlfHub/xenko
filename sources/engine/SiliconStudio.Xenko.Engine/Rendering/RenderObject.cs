using System;
using System.Threading;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class VisibilityObject
    {
        public RenderObject RenderObject;
    }

    /// <summary>
    /// Describes something that can be rendered by a <see cref="RootRenderFeature"/>.
    /// </summary>
    public abstract class RenderObject
    {
        private static int currentIndex;

        public bool Enabled = true;
        public EntityGroup RenderGroup;

        public BoundingBoxExt BoundingBox;

        // Kept in cache to quickly know if RenderPerFrameNode was already generated
        public RootRenderFeature RenderFeature;
        public ObjectNodeReference ObjectNode;
        public StaticObjectNodeReference StaticObjectNode = StaticObjectNodeReference.Invalid;

        public StaticObjectNodeReference VisibilityObjectNode = StaticObjectNodeReference.Invalid;

        public ActiveRenderStage[] ActiveRenderStages;
        public uint StateSortKey;
        public readonly int Index = Interlocked.Increment(ref currentIndex);

        // TODO: Switch to a "StaticPropertyContainer" that will be optimized by assembly processor
        //public PropertyContainer Tags;
    }
}