﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A button gesture generated from an <see cref="IAxisGesture"/>, with a customizable threshold
    /// </summary>
    [DataContract]
    public class AxisButtonGesture : ButtonGestureBase
    {
        /// <summary>
        /// The threshold value the axis needs to reach in order to trigger a button press
        /// </summary>
        public float Threshold = 0.5f;

        private IAxisGesture axis;
        
        /// <summary>
        /// The axis that triggers this button
        /// </summary>
        public IAxisGesture Axis
        {
            get { return axis; }
            set
            {
                if (axis != null)
                {
                    axis.Changed -= AxisOnChanged;
                    RemoveChild(axis);
                }
                axis = value;
                if (axis != null)
                {
                    AddChild(axis);
                    axis.Changed += AxisOnChanged;
                }
            }
        }

        private void AxisOnChanged(object sender, AxisGestureEventArgs args)
        {
            var state = args.State > Threshold ? ButtonState.Down : ButtonState.Up;
            UpdateButton(state, args.Device);
        }
        
        public override string ToString()
        {
            return $"{nameof(Threshold)}: {Threshold}";
        }
    }
}