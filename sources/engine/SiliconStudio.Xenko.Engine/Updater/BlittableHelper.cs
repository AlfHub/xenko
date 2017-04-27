// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Various helpers for blittable types.
    /// </summary>
    // TODO: We should switch to something determined at compile time with assembly processor?
    internal static class BlittableHelper
    {
        private static Dictionary<Type, bool> BlittableTypesCache = new Dictionary<Type, bool>();

        // TODO: Performance: precompute this in AssemblyProcessor
        public static bool IsBlittable(Type type)
        {
            lock (BlittableTypesCache)
            {
                bool blittable;
                try
                {
                    // Check cache
                    if (BlittableTypesCache.TryGetValue(type, out blittable))
                        return blittable;

                    // Class test
                    if (!type.GetTypeInfo().IsValueType)
                    {
                        blittable = false;
                    }
                    else
                    {
                        // Non-blittable types cannot allocate pinned handle
                        GCHandle.Alloc(Activator.CreateInstance(type), GCHandleType.Pinned).Free();
                        blittable = true;
                    }
                }
                catch
                {
                    blittable = false;
                }

                // Register it for next time
                BlittableTypesCache[type] = blittable;
                return blittable;
            }
        }
    }
}
