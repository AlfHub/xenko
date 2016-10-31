﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    public interface IInputDevice : IDisposable
    {
        /// <summary>
        /// Name for this device
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// Unique Id for this device
        /// </summary>
        Guid Id { get; }
        
        /// <summary>
        /// Device priority, larger means higher priority when selecting the first device of some type
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// Updates the input device, filling the list <see cref="inputEvents"/> with input events that were generated by this device this frame
        /// </summary>
        /// <remarks>Input devices are always updated after their respective input source</remarks>
        /// <param name="inputEvents">A list that gets filled with input events that were generated since the last frame</param>
        void Update(List<InputEvent> inputEvents);
    }
}