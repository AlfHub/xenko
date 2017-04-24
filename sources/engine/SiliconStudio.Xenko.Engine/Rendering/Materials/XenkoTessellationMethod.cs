// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describe the different tessellation methods used in Xenko.
    /// </summary>
    [Flags]
    public enum XenkoTessellationMethod
    {
        /// <summary>
        /// No tessellation
        /// </summary>
        None = 0,

        /// <summary>
        /// Flat tessellation. Also known as dicing tessellation.
        /// </summary>
        Flat = 1,

        /// <summary>
        /// Point normal tessellation.
        /// </summary>
        PointNormal = 1,

        /// <summary>
        /// Adjacent edge average.
        /// </summary>
        AdjacentEdgeAverage = 2,
    }

    public static class XenkoTessellationMethodExtensions
    {
        public static bool PerformsAdjacentEdgeAverage(this XenkoTessellationMethod method)
        {
            return (method & XenkoTessellationMethod.AdjacentEdgeAverage) != 0;
        }

        public static PrimitiveType GetPrimitiveType(this XenkoTessellationMethod method)
        {
            if((method & XenkoTessellationMethod.PointNormal) == 0)
                return PrimitiveType.TriangleList;

            var controlsCount = method.PerformsAdjacentEdgeAverage() ? 12 : 3;
            return PrimitiveType.PatchList.ControlPointCount(controlsCount);
        }
    }
}
