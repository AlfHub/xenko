﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Keys used by <see cref="AmbientOcclusionBlur"/> and AmbientOcclusionBlurEffect xkfx
    /// </summary>
    internal static class AmbientOcclusionBlurKeys
    {
        public static readonly PermutationParameterKey<int> Count = ParameterKeys.NewPermutation<int>(9);

        public static readonly PermutationParameterKey<bool> VerticalBlur = ParameterKeys.NewPermutation<bool>();

        public static readonly PermutationParameterKey<float> BlurScale = ParameterKeys.NewPermutation<float>(2f);

        public static readonly PermutationParameterKey<float> EdgeSharpness = ParameterKeys.NewPermutation<float>(4f);
    }
}
