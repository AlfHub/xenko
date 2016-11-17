﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Maps <see cref="InputGestureBase"/>s to <see cref="InputAction"/>s. This allows the user to change what physical inputs map to a certain input gesture
    /// </summary>
    public class InputActionMapping : IDisposable
    {
        internal InputManager InputManager;
        // NOTE: Gestures are compared by reference here since normally they are equal if they are monitoring the same thing
        private readonly HashSet<IInputGesture> gestures = new HashSet<IInputGesture>(ReferenceEqualityComparer<IInputGesture>.Default);
        private readonly List<InputAction> inputActions = new List<InputAction>();
        private readonly Dictionary<string, InputAction> inputActionsByName = new Dictionary<string, InputAction>();

        /// <summary>
        /// Creates a new instance of <see cref="InputActionMapping"/>.
        /// </summary>
        /// <param name="inputManager">The <see cref="Input.InputManager"/> which this mapping receives events from</param>
        public InputActionMapping(InputManager inputManager)
        {
            if (inputManager == null) throw new ArgumentNullException(nameof(inputManager));
            this.InputManager = inputManager;
            inputManager.PreUpdateInput += OnPreUpdateInput;
        }
        
        public void Dispose()
        {
            if (InputManager == null) return; // Already disposed

            // Remove PreUpdate handler
            InputManager.PreUpdateInput -= OnPreUpdateInput;

            // Remove all activated gestures
            foreach(var action in inputActions)
            {
                action.ActionMapping = null;
                foreach (var gesture in action.Gestures)
                {
                    InputManager.ActivatedGestures.Remove(gesture);
                }
            }

            // Avoids adding any new handlers later on
            InputManager = null;
        }

        /// <summary>
        /// A list of actions that are in this action mapping
        /// </summary>
        public IReadOnlyList<InputAction> Actions => inputActions;

        /// <summary>
        /// A set of gestures that is already bound
        /// </summary>
        /// <remarks>This function is not cached, it will traverse all the gestures on this action mapping to list all gestures</remarks>
        public HashSet<IInputGesture> BoundGestures
        {
            get
            {
                HashSet<IInputGesture> gestures = new HashSet<IInputGesture>();
                foreach (var action in Actions)
                {
                    foreach (var gesture in action.Gestures)
                    {
                        var gestureBase = gesture as InputGestureBase;
                        gestureBase.GetGesturesRecursive(gestures);
                    }
                }
                return gestures;
            }
        }

        /// <summary>
        /// Use this to serialize/deserialize the action mapping
        /// </summary>
        public Dictionary<string, List<InputGestureBase>> Bindings
        {
            get
            {
                // Collect all the gesture bindings into a key/value pair mapping
                Dictionary<string, List<InputGestureBase>> settings = new Dictionary<string, List<InputGestureBase>>();
                foreach (var pair in inputActionsByName)
                {
                    settings.Add(pair.Key, pair.Value.Gestures.OfType<InputGestureBase>().ToList());
                }
                return settings;
            }
            set
            {
                // Load the new gesture bindings
                foreach (var pair in value)
                {
                    InputAction action;
                    if (inputActionsByName.TryGetValue(pair.Key, out action))
                    {
                        action.Gestures.Clear();
                        action.Gestures.AddRange(pair.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the controller index for this player, this will determine which gamepad will trigger actions for this input component
        /// </summary>
        public int ControllerIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets if mouse input will trigger actions for this input component
        /// </summary>
        public bool AcceptMouse { get; set; } = true;

        /// <summary>
        /// Gets or sets if Keyboard input will trigger actions for this input component
        /// </summary>
        public bool AcceptKeyboard { get; set; } = true;

        /// <summary>
        /// Updates the input gestures and send input action events
        /// </summary>
        /// <param name="elapsedTime">The elapsed time since the last update</param>
        public void Update(TimeSpan elapsedTime)
        {
            // Update actions
            foreach (var action in inputActions)
                action.Update(elapsedTime);
        }

        /// <summary>
        /// Adds a new input action to update
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(InputAction action)
        {
            string name = action.MappingName;
            if (action.ActionMapping != null || inputActions.Contains(action))
                throw new InvalidOperationException("Action was already added to a mapping");
            if (inputActionsByName.ContainsKey(name))
                throw new InvalidOperationException("Can't add binding, a binding with the same name already exists");

            // Bind action to mapper so that when the gesture changes later it will call this function again
            action.MappingName = name;
            action.ActionMapping = this;
            inputActions.Add(action);
            inputActionsByName.Add(name, action);

            // Add gestures to the input managers
            foreach (var gesture in action.Gestures.OfType<InputGestureBase>())
            {
                InputManager.ActivatedGestures.Add(gesture);
            }
        }

        /// <summary>
        /// Removes an action binding
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAction(InputAction action)
        {
            if (action.ActionMapping == null || !inputActions.Contains(action))
                throw new InvalidOperationException("Action was not added to this mapping");

            // Remove gestures from event routers
            foreach (var gesture in action.Gestures.OfType<InputGestureBase>())
            {
                InputManager.ActivatedGestures.Remove(gesture);
            }

            inputActions.Remove(action);
            inputActionsByName.Remove(action.MappingName);
            action.ActionMapping = null;
            action.MappingName = "";
        }

        /// <summary>
        /// Tries to find an action in the default configuration with <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name of the action to find, speciefied in <see cref="InputAction.MappingName"/></param>
        /// <returns>The found action or <c>null</c></returns>
        public InputAction TryGetAction(string name)
        {
            InputAction action;
            inputActionsByName.TryGetValue(name, out action);
            return action;
        }

        /// <summary>
        /// Removes all bindings that are currently bound
        /// </summary>
        public void ClearBindings()
        {
            InputAction[] actionsToRemove = inputActions.ToArray();
            foreach (var action in actionsToRemove)
            {
                RemoveAction(action);
            }
            if (inputActionsByName.Count != 0) throw new Exception("Failed to correctly clear bindings");
            if (gestures.Count != 0) throw new Exception("Failed to correctly clear bindings");
            if (inputActions.Count != 0) throw new Exception("Failed to correctly clear bindings");
        }
        
        /// <summary>
        /// Called before input update to reset gesture states
        /// </summary>
        private void OnPreUpdateInput(object sender, InputPreUpdateEventArgs e)
        {
            foreach (var action in inputActions)
            {
                action.PreUpdate(e.GameTime.Elapsed);
            }
        }

    }
}