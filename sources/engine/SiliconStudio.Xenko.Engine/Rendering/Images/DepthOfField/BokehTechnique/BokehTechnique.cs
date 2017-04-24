// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Rendering.Images
{

    /// <summary>
    /// Techniques available to perform a DoF effect on a level.
    /// The technique directly affects the visual result (bokeh shape) as well as the performance. 
    /// </summary>
    public enum BokehTechnique
    {
        /// <summary>
        /// Circular blur using a Gaussian. 
        /// </summary>
        /// <remarks>
        /// Fast and cheap technique but the final bokeh shapes are not very realistic.
        /// </remarks>
        /// <userdoc>Use circular Gaussian blur to render the bokehs. This technique produce circular bokehs.
        /// It is fast and cheap technique but not very realistic.</userdoc>
        CircularGaussian,

        /// <summary>
        /// Hexagonal blur using the McIntosh technique.
        /// </summary>
        /// <userdoc>Use the McIntosh hexagonal blur to render the bokehs. This technique produce hexagonal bokehs</userdoc>
        HexagonalMcIntosh,

        /// <summary>
        /// Hexagonal blur using a combination of 3 rhombi blurs. 
        /// </summary>
        /// <userdoc>Use a combination of 3 rhombi blurs to render the bokehs. This technique produce hexagonal bokehs</userdoc>
        HexagonalTripleRhombi
    }


    // Extension methods to directly instantiate a blur image effect from a bokeh technique name.
    public static class BokehTechniqueExtensions
    {
        /// <summary>
        /// Instantiates a new <see cref="BokehBlur"/> from a technique name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A Bokeh blur corresponding to the tehcnique specified.</returns>
        public static BokehBlur ToBlurInstance(this BokehTechnique name)
        {
            switch (name)
            {
                case BokehTechnique.CircularGaussian:
                    return new GaussianBokeh();

                case BokehTechnique.HexagonalMcIntosh:
                    return new McIntoshBokeh();

                case BokehTechnique.HexagonalTripleRhombi:
                    return new TripleRhombiBokeh();

                default:
                    throw new ArgumentOutOfRangeException("Unknown bokeh technique: " + name);
            }
        }
    }
       
}
