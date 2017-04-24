// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines how to set and get values from a property of a given reference type for the <see cref="UpdateEngine"/>.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    public class UpdatablePropertyObject<T> : UpdatableProperty
    {
        public UpdatablePropertyObject(IntPtr getter, IntPtr setter) : base(getter, setter)
        {
        }

        /// <inheritdoc/>
        public override Type MemberType
        {
            get { return typeof(T); }
        }

        /// <inheritdoc/>
        public override void GetBlittable(IntPtr obj, IntPtr data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void SetBlittable(IntPtr obj, IntPtr data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void SetStruct(IntPtr obj, object data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IntPtr GetStructAndUnbox(IntPtr obj, object data)
        {
            throw new NotImplementedException();
        }
    }
}
