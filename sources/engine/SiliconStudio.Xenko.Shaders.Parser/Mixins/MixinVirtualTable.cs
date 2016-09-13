// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Shaders.Ast.Xenko;
using SiliconStudio.Xenko.Shaders.Parser.Utility;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Xenko.Shaders.Parser.Mixins
{
    internal class MixinVirtualTable : ShaderVirtualTable
    {
        #region Public properties

        /// <summary>
        /// List of all declared methods
        /// </summary>
        public HashSet<MethodDeclarationShaderCouple> Methods { get; private set; }

        /// <summary>
        /// List of all declared Variables
        /// </summary>
        public HashSet<VariableShaderCouple> Variables { get; private set; }

        /// <summary>
        /// List of all the structure definitions
        /// </summary>
        public List<StructType> StructureTypes { get; private set; } // list instead of hashset because order can be important

        /// <summary>
        /// List of all the Typedefs
        /// </summary>
        public List<Typedef> Typedefs { get; private set; } // list instead of hashset because order can be important

        #endregion

        #region Constructor

        public MixinVirtualTable()
        {
            Methods = new HashSet<MethodDeclarationShaderCouple>();
            Variables = new HashSet<VariableShaderCouple>();
            StructureTypes = new List<StructType>();
            Typedefs = new List<Typedef>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Merge with a local virtual table =  need to check override keywords
        /// </summary>
        /// <param name="virtualTable">the virtual table to add</param>
        /// <param name="mixinName">the name of the mixin</param>
        /// <param name="log">the error logger</param>
        public void MergeWithLocalVirtualTable(MixinVirtualTable virtualTable, string mixinName, LoggerResult log)
        {
            foreach (var method in virtualTable.Methods)
            {
                var methodDecl = Methods.LastOrDefault(x => x.Method.IsSameSignature(method.Method));
                if (methodDecl != null)
                {
                    var isBaseMethod = method.Shader.BaseClasses.Any(x => x.Name.Text == methodDecl.Shader.Name.Text);

                    if (isBaseMethod)
                    {
                        if (methodDecl.Method is MethodDefinition)
                        {
                            if (!method.Method.Qualifiers.Contains(XenkoStorageQualifier.Override))
                            {
                                log.Error(XenkoMessageCode.ErrorMissingOverride, method.Method.Span, method.Method, mixinName);
                                continue;
                            }
                        }
                        else if (method.Method.Qualifiers.Contains(XenkoStorageQualifier.Override))
                        {
                            log.Error(XenkoMessageCode.ErrorOverrideDeclaration, method.Method.Span, method.Method, mixinName);
                            continue;
                        }
                    }

                    Methods.Remove(methodDecl);
                }
                else
                {
                    if (method.Method.Qualifiers.Contains(XenkoStorageQualifier.Override))
                    {
                        log.Error(XenkoMessageCode.ErrorNoMethodToOverride, method.Method.Span, method.Method, mixinName);
                        continue;
                    }
                }

                Methods.Add(method);
                
                // TODO: handle declarations vs definitions
            }

            Variables.UnionWith(virtualTable.Variables.Where(x => !Variables.Contains(x)));
            StructureTypes.AddRange(virtualTable.StructureTypes.Where(x => !StructureTypes.Contains(x)));
            Typedefs.AddRange(virtualTable.Typedefs.Where(x => !Typedefs.Contains(x)));
        }

        /// <summary>
        /// Check the name conflict between the two virtual tables
        /// </summary>
        public bool CheckNameConflict(MixinVirtualTable virtualTable, LoggerResult log)
        {
            var conflict = false;

            foreach (var variable in virtualTable.Variables.Where(variable => Variables.Any(x => x.Variable.Name.Text == variable.Variable.Name.Text)))
            {
                log.Error(XenkoMessageCode.ErrorVariableNameConflict, variable.Variable.Span, variable.Variable, "");
                conflict = true;
            }

            return conflict;
        }

        #endregion
    }
}