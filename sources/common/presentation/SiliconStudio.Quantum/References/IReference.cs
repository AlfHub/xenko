﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.References
{
    public interface IReference : IEquatable<IReference>
    {
        /// <summary>
        /// Gets the data object targeted by this reference, if available.
        /// </summary>
        object ObjectValue { get; }

        /// <summary>
        /// Gets this object casted as a <see cref="ObjectReference"/>.
        /// </summary>
        ObjectReference AsObject { get; }

        /// <summary>
        /// Gets this object casted as a <see cref="ReferenceEnumerable"/>.
        /// </summary>
        ReferenceEnumerable AsEnumerable { get; }
    }
}
