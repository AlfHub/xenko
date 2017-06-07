// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    public class GamePadSimulated : GamePadDeviceBase 
    {
        public GamePadSimulated(InputSourceSimulated source)
        {
            ProductId = new Guid("B540474D-F8CC-4D27-B57E-7E87272FD9E6");
            Id = Guid.NewGuid();
            Source = source;
            State = new GamePadState();
        }

        public override string Name { get; }
        public override Guid Id { get; }
        public override IInputSource Source { get; }
        public override Guid ProductId { get; }
        public override GamePadState State { get; }

        private List<InputEvent> PendingEvents = new List<InputEvent>();

        public void SetButton(GamePadButton button, bool state)
        {
            // Check for only 1 bit
            if((button & (button-1)) != 0)
                throw new InvalidOperationException("Can not set more than one button at a time");

            var buttonEvent = InputEventPool<GamePadButtonEvent>.GetOrCreate(this);
            buttonEvent.Button = button;
            buttonEvent.IsDown = state;
            PendingEvents.Add(buttonEvent);
        }

        public void SetAxis(GamePadAxis axis, float value)
        {
            var axisEvent = InputEventPool<GamePadAxisEvent>.GetOrCreate(this);
            axisEvent.Axis = axis;
            axisEvent.Value = value;
            PendingEvents.Add(axisEvent);
        }

        public override void Update(List<InputEvent> inputEvents)
        {
            ClearButtonStates();

            foreach (var evt in PendingEvents)
            {
                State.Update(evt);

                var buttonEvent = evt as GamePadButtonEvent;
                if(buttonEvent != null)
                    UpdateButtonState(buttonEvent);
            }

            inputEvents.AddRange(PendingEvents);
            PendingEvents.Clear();
        }

        public override void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight)
        {
        }
    }
}
