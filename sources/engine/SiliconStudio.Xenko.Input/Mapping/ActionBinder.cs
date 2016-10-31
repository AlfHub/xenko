﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Receives a single or chain of inputs in order to create a <see cref="IInputGesture"/> that represents the given input
    /// </summary>
    public abstract class ActionBinder : IInputEventListener, IDisposable
    {
        protected InputManager InputManager;
        protected readonly HashSet<IInputGesture> UsedGestures;

        /// <summary>
        /// Initialize the <see cref="ActionBinder"/> base class
        /// </summary>
        /// <param name="inputManager">The input manager that can be used to watch for inputs</param>
        /// <param name="usedGestures">A set of already used gesture that are filtered out from the input, can be null</param>
        protected ActionBinder(InputManager inputManager, HashSet<IInputGesture> usedGestures = null)
        {
            UsedGestures = usedGestures ?? new HashSet<IInputGesture>();
            InputManager = inputManager;
        }

        public void Dispose()
        {
            if (!Done) InputManager.RemoveListener(this);
        }

        /// <summary>
        /// The number of bindings
        /// </summary>
        public abstract int NumBindings { get; }
        
        /// <summary>
        /// <c>true</c> if this binder accepts axes input events, such as mouse X axis or gamepad axis 1
        /// </summary>
        public virtual bool AcceptsAxes { get; private set; } = true;

        /// <summary>
        /// <c>true</c> if this binder accepts button input events, such as keyboard keys and mouse buttons
        /// </summary>
        public virtual bool AcceptsButtons { get; private set; } = true;

        /// <summary>
        /// <c>true</c> if this binder accepts combinded directional input events, such as mouse X,Y movement and gamepad pov controller
        /// </summary>
        public virtual bool AcceptsDirections { get; private set; } = true;

        /// <summary>
        /// <c>true</c> if binding has received all the required inputs to bind the action
        /// </summary>
        public bool Done { get; private set; }

        /// <summary>
        /// Index of the current input
        /// </summary>
        public int Index { get; private set; } = 0;
        
        /// <summary>
        /// The generated gesture
        /// </summary>
        public IInputGesture TargetGesture { get; protected set; }

        /// <summary>
        /// Moves the input being detected by a given amount (default = 1). 
        /// If the number of inputs has reached <see cref="NumBindings"/>, <see cref="Done"/> will be set to true and the action will unbind itself from the input manager
        /// </summary>
        /// <param name="amount">The number of inputs to advance</param>
        protected void Advance(int amount = 1)
        {
            if (!Done)
            {
                Index += amount;
                Done = Index >= NumBindings;
                if (Done)
                {
                    InputManager.RemoveListener(this);
                    Index = NumBindings - 1;
                }
            }
        }
    }
}