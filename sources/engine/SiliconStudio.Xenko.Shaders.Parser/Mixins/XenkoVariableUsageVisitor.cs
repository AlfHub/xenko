﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Shaders.Ast.Xenko;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    internal class XenkoVariableUsageVisitor : ShaderWalker
    {
        private Dictionary<Variable, bool> VariablesUsages;

        public XenkoVariableUsageVisitor(Dictionary<Variable, bool> variablesUsages)
            : base(false, false)
        {
            if (variablesUsages == null)
                VariablesUsages = new Dictionary<Variable, bool>();
            else
                VariablesUsages = variablesUsages;
        }

        public void Run(ShaderClassType shaderClassType)
        {
            Visit(shaderClassType);
        }

        public override void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            base.Visit(variableReferenceExpression);
            CheckUsage(variableReferenceExpression.TypeInference.Declaration as Variable);
        }

        public override void Visit(MemberReferenceExpression memberReferenceExpression)
        {
            base.Visit(memberReferenceExpression);
            CheckUsage(memberReferenceExpression.TypeInference.Declaration as Variable);
        }

        private void CheckUsage(Variable variable)
        {
            if (variable == null)
                return;

            if (VariablesUsages.ContainsKey(variable))
                VariablesUsages[variable] = true;
        }
    }
}
