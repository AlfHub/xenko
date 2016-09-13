// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using SiliconStudio.Core;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A Source location.
    /// </summary>
    [DataContract]
    public struct SourceLocation
    {
        #region Constants and Fields

        /// <summary>
        /// Filename source.
        /// </summary>
        public string FileSource;

        /// <summary>
        /// Absolute position in the file.
        /// </summary>
        public int Position;

        /// <summary>
        /// Line in the file (1-based).
        /// </summary>
        public int Line;

        /// <summary>
        /// Column in the file (1-based).
        /// </summary>
        public int Column;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceLocation"/> struct.
        /// </summary>
        /// <param name="fileSource">The file source.</param>
        /// <param name="position">The position.</param>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        public SourceLocation(string fileSource, int position, int line, int column)
        {
            FileSource = fileSource;
            Position = position;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceLocation"/> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="line">The line.</param>
        /// <param name="column">The column.</param>
        public SourceLocation(int position, int line, int column)
            : this()
        {
            Position = position;
            Line = line;
            Column = column;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ToString(false);
        }

        public string ToString(bool useShortFileName)
        {
            return string.Format("{0}({1},{2})", FileSource ?? string.Empty, Line, Column);
        }

        #endregion
    }
}