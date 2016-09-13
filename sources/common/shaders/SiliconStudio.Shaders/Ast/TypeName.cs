// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A typeless reference.
    /// </summary>
    public partial class TypeName : TypeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeName"/> class.
        /// </summary>
        public TypeName()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeName"/> class.
        /// </summary>
        /// <param name="typeBase">The type base.</param>
        public TypeName(TypeBase typeBase)
            : base(typeBase.Name)
        {
            TypeInference.TargetType = typeBase;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeName"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public TypeName(Identifier name) : base(name)
        {
        }
    }
}