﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes a sensor that implements Enabled/Disable and provides a name/guid set from constructor
    /// </summary>
    internal class NamedSensor : ISensorDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedSensor"/> class.
        /// </summary>
        protected NamedSensor(IInputSource source, string systemName, string sensorName)
        {
            Source = source;
            Name = $"{systemName} {sensorName} Sensor";
            Id = InputDeviceUtils.DeviceNameToGuid(systemName + sensorName);
        }

        public string Name { get; }

        public Guid Id { get; }

        public int Priority { get; set; }

        public IInputSource Source { get; }

        public bool IsEnabled { get; set; }

        public void Update(List<InputEvent> inputEvents)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}