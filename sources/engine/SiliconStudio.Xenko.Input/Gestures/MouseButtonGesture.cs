﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A gesture that detects a mouse button being pressed or released
    /// </summary>
    [DataContract]
    public class MouseButtonGesture : ButtonGestureBase, IInputEventListener<MouseButtonEvent>
    {
        /// <summary>
        /// Button used for this gesture
        /// </summary>
        public MouseButton Button;

        public MouseButtonGesture()
        {
        }

        public MouseButtonGesture(MouseButton button)
        {
            Button = button;
        }
        
        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.Button == Button)
                UpdateButton(inputEvent.State, inputEvent.Device);
        }

        public override string ToString()
        {
            return $"{nameof(Button)}: {Button}";
        }

        protected bool Equals(MouseButtonGesture other)
        {
            return Button == other.Button;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MouseButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return (int)Button;
        }
    }
}