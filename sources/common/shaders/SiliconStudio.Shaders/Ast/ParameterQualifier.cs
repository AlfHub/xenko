﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A Storage qualifier.
    /// </summary>
    public static class ParameterQualifier
    {
        #region Constants and Fields

        /// <summary>
        ///   In modifier, only for method parameters.
        /// </summary>
        public static readonly Qualifier In = new Qualifier("in");

        /// <summary>
        ///   InOut Modifier, only for method parameters.
        /// </summary>
        public static readonly Qualifier InOut = new Qualifier("inout");

        /// <summary>
        ///   Out modifier, only for method parameters.
        /// </summary>
        public static readonly Qualifier Out = new Qualifier("out");

        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the specified enum name.
        /// </summary>
        /// <param name="enumName">
        /// Name of the enum.
        /// </param>
        /// <returns>
        /// A parameter qualifier
        /// </returns>
        public static Qualifier Parse(string enumName)
        {
            if (enumName == (string)In.Key)
                return In;
            if (enumName == (string)InOut.Key)
                return InOut;
            if (enumName == (string)Out.Key)
                return Out;

            throw new ArgumentException(string.Format("Unable to convert [{0}] to qualifier", enumName), "key");
        }

        #endregion
    }
}