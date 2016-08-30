﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Diagnostics;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.UI.Controls
{
    /// <summary>
    /// Represents the base class for user interface (UI) controls. 
    /// </summary>
    [DataContract(nameof(Control))]
    [DebuggerDisplay("Control - Name={Name}")]
    public abstract class Control : UIElement
    {
        protected Thickness padding = Thickness.UniformCuboid(0);

        /// <summary>
        /// Gets or sets the padding inside a control.
        /// </summary>
        /// <userdoc>The padding inside a control.</userdoc>
        [DataMember]
        [Display(category: LayoutCategory)]
        public Thickness Padding
        {
            get { return padding; }
            set
            {
                padding = value;
                InvalidateMeasure();
            }
        }
    }
}
