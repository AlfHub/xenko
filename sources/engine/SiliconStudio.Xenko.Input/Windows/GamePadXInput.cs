﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.XInput;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class GamePadXInput : GameControllerDeviceBase, IGamePadDevice
    {
        private readonly Controller controller;
        private State xinputState;
        private GamePadState state;
        private short[] lastAxisState = new short[6];

        public GamePadXInput(Controller controller, Guid id, int index)
        {
            this.controller = controller;
            IndexInternal = index;
            Id = id;
            state = new GamePadState();
            
            InitializeButtonStates();
        }

        public override string DeviceName => $"XInput Controller {IndexInternal}";
        public override Guid Id { get; }
        public GamePadState State => state;

        public override IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; } = new GameControllerButtonInfo[] { };
        public override IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; } = new GameControllerAxisInfo[] { };
        public override IReadOnlyList<PovControllerInfo> PovControllerInfos { get; } = new PovControllerInfo[] { };

        public override void Update(List<InputEvent> inputEvents)
        {
            if (Disposed)
                return;
            
            if (controller.GetState(out xinputState))
            {
                // DPad/Shoulder/Thumb/Option buttons
                for (int i = 0; i < 16; i++)
                {
                    int mask = 1 << i;
                    var masked = ((int)xinputState.Gamepad.Buttons & mask);
                    if (masked != ((int)state.Buttons & mask))
                    {
                        ButtonState buttonState = (masked != 0) ? ButtonState.Down : ButtonState.Up;
                        GamePadButtonEvent buttonEvent = InputEventPool<GamePadButtonEvent>.GetOrCreate(this);
                        buttonEvent.State = buttonState;
                        buttonEvent.Button = (GamePadButton)mask; // 1 to 1 mapping with XInput buttons
                        inputEvents.Add(buttonEvent);
                        state.Update(buttonEvent);
                    }
                }
                
                // Axes
                if (ChangeAxis(0, xinputState.Gamepad.LeftThumbX))
                    inputEvents.Add(CreateAxisEvent(GamePadAxis.LeftThumbX, xinputState.Gamepad.LeftThumbX / 32768.0f));
                if (ChangeAxis(1, xinputState.Gamepad.LeftThumbY))
                    inputEvents.Add(CreateAxisEvent(GamePadAxis.LeftThumbY, xinputState.Gamepad.LeftThumbY / 32768.0f));

                if (ChangeAxis(2, xinputState.Gamepad.RightThumbX))
                    inputEvents.Add(CreateAxisEvent(GamePadAxis.RightThumbX, xinputState.Gamepad.RightThumbX / 32768.0f));
                if (ChangeAxis(3, xinputState.Gamepad.RightThumbY))
                    inputEvents.Add(CreateAxisEvent(GamePadAxis.RightThumbY, xinputState.Gamepad.RightThumbY / 32768.0f));

                if (ChangeAxis(4, xinputState.Gamepad.LeftTrigger))
                    inputEvents.Add(CreateAxisEvent(GamePadAxis.LeftTrigger, xinputState.Gamepad.LeftTrigger / 255.0f));
                if (ChangeAxis(5, xinputState.Gamepad.RightTrigger))
                    inputEvents.Add(CreateAxisEvent(GamePadAxis.RightTrigger, xinputState.Gamepad.RightTrigger / 255.0f));
            }

            if (!controller.IsConnected)
            {
                Dispose();
            }

            // Fire events
            base.Update(inputEvents);
        }

        public void SetVibration(float leftMotor, float rightMotor)
        {
            if (Disposed)
                return;
            try
            {
                leftMotor = MathUtil.Clamp(leftMotor, 0.0f, 1.0f);
                rightMotor = MathUtil.Clamp(rightMotor, 0.0f, 1.0f);
                var vibration = new Vibration
                {
                    LeftMotorSpeed = (ushort)(leftMotor * 65535.0f),
                    RightMotorSpeed = (ushort)(rightMotor * 65535.0f)
                };
                controller.SetVibration(vibration);
            }
            catch (SharpDXException)
            {
                Dispose();
            }
        }

        public void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight)
        {
            SetVibration((smallLeft + largeLeft) * 0.5f, (smallRight + largeRight) * 0.5f);
        }

        private bool ChangeAxis(int index, short newValue)
        {
            if (lastAxisState[index] != newValue)
            {
                lastAxisState[index] = newValue;
                return true;
            }
            return false;
        }

        private GamePadAxisEvent CreateAxisEvent(GamePadAxis axis, float newValue)
        {
            GamePadAxisEvent axisEvent = InputEventPool<GamePadAxisEvent>.GetOrCreate(this);
            axisEvent.Value = newValue;
            axisEvent.Axis = axis;
            state.Update(axisEvent);
            return axisEvent;
        }
    }
}

#endif