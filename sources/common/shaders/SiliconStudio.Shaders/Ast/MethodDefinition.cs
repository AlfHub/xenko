﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A method definition with a body of statements.
    /// </summary>
    public partial class MethodDefinition : MethodDeclaration, IScopeContainer
    {
        private MethodDeclaration declaration;

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinition"/> class.
        /// </summary>
        public MethodDefinition()
        {
            Body = new StatementList();
            declaration = this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodDefinition"/> class.
        /// </summary>
        /// <param name="returntype">The returntype.</param>
        /// <param name="name">The name.</param>
        public MethodDefinition(TypeBase returntype, string name) : this()
        {
            ReturnType = returntype;
            Name = new Identifier(name);
            declaration = this;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the declaration.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        [VisitorIgnore]
        public MethodDeclaration Declaration { get { return declaration; } set { declaration = value; } }

        /// <summary>
        ///   Gets or sets the list of statements.
        /// </summary>
        /// <value>
        ///   The list of statements.
        /// </value>
        public StatementList Body { get; set; }


        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            base.Childrens();
            ChildrenList.Add(Body);
            return ChildrenList;
        }

        #endregion
    }
}