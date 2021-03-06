// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// An implementation of the <see cref="AssetFactory{T}"/> class that uses the default public parameterless constructor
    /// of the associated asset type.
    /// </summary>
    /// <typeparam name="T">The type of asset this factory can create.</typeparam>
    public class DefaultAssetFactory<T> : AssetFactory<T> where T : Asset
    {
        public static T Create()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }

        /// <inheritdoc/>
        public override T New()
        {
            if (typeof(T).GetConstructor(Type.EmptyTypes) == null)
                throw new InvalidOperationException("The associated asset type does not have a public parameterless constructor.");

            return Create();
        }
    }
}
