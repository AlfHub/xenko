﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A reference to a variable.
    /// </summary>
    public partial class TypeReferenceExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReferenceExpression"/> class.
        /// </summary>
        public TypeReferenceExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReferenceExpression"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public TypeReferenceExpression(TypeBase type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public TypeBase Type { get; set; }

        /// <summary>
        /// Gets or sets the declaration.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        public IDeclaration Declaration { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Type);
            return ChildrenList;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
