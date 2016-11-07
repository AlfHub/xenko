﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A device such as a keyboard that supports text input. This can be a windows keyboard with IME support or a touch keyboard on a smartphone device
    /// </summary>
    public interface ITextInputDevice : IInputDevice
    {
        /// <summary>
        /// Enables the device specific way of entering text input
        /// </summary>
        void EnabledTextInput();
        
        /// <summary>
        /// Disables the device specific way of entering text input
        /// </summary>
        void DisableTextInput();
    }
}