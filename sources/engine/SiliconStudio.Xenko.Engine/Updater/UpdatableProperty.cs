// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines how to set and get values from a property for the <see cref="UpdateEngine"/>.
    /// </summary>
    public abstract class UpdatableProperty : UpdatablePropertyBase
    {
        public readonly IntPtr Getter;
        public readonly IntPtr Setter;

        protected UpdatableProperty(IntPtr getter, IntPtr setter)
        {
            Getter = getter;
            Setter = setter;
        }

        /// <summary>
        /// Gets a reference object from a property.
        /// </summary>
        /// <param name="obj">The object encoded as a native pointer (<see cref="UpdateEngine"/> will make sure it is pinned).</param>
        /// <returns>The object value from the property.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetObject(IntPtr obj)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldarg.0
            ldfld native int class SiliconStudio.Xenko.Updater.UpdatableProperty::Getter
            calli instance object()
            ret
#endif
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a reference object from a property.
        /// </summary>
        /// <param name="obj">The object encoded as a native pointer (<see cref="UpdateEngine"/> will make sure it is pinned).</param>
        /// <param name="data">The object value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetObject(IntPtr obj, object data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            ldarg obj
            ldarg data
            ldarg.0
            ldfld native int class SiliconStudio.Xenko.Updater.UpdatableProperty::Setter
            calli instance void(object)
            ret
#endif
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        internal override UpdateOperationType GetSetOperationType()
        {
            if (MemberType.GetTypeInfo().IsValueType)
            {
                if (BlittableHelper.IsBlittable(MemberType))
                    return UpdateOperationType.ConditionalSetBlittablePropertyBase;

                return UpdateOperationType.ConditionalSetStructPropertyBase;
            }
            else
            {
                return UpdateOperationType.ConditionalSetObjectProperty;
            }
        }

        /// <inheritdoc/>
        internal override UpdateOperationType GetEnterOperationType()
        {
            if (MemberType.GetTypeInfo().IsValueType)
            {
                return UpdateOperationType.EnterStructPropertyBase;
            }
            else
            {
                return UpdateOperationType.EnterObjectProperty;
            }
        }
    }
}
