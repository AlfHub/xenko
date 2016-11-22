﻿using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;

namespace ThirdPersonPlatformer.Core
{
    public static class InputManagerExtensions
    {
        public static bool IsGamePadButtonDown(this InputManager input, GamePadButton button, int index)
        {
            if (input.GamePadCount < index)
                return false;

            return (input.GetGamePad(index).State.Buttons & button) == button;
        }

        public static bool IsGamePadButtonDownAny(this InputManager input, GamePadButton button)
        {
            for (int i = 0; i < input.GamePadCount; i++)
            {
                if ((input.GetGamePad(i).State.Buttons & button) == button)
                    return true;
            }

            return false;
        }

        public static Vector2 GetLeftThumb(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.LeftThumb : Vector2.Zero;
        }

        public static Vector2 GetLeftThumbAny(this InputManager input, float deadZone)
        {
            int totalCount = 0;
            Vector2 totalMovement = Vector2.Zero;
            for (int i = 0; i < input.GamePadCount; i++)
            {
                var leftVector = input.GetGamePad(i).State.LeftThumb;
                if (leftVector.Length() >= deadZone)
                {
                    totalCount++;
                    totalMovement += leftVector;
                }
            }

            return (totalCount > 1) ? (totalMovement / totalCount) : totalMovement;
        }

        public static Vector2 GetRightThumb(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.RightThumb : Vector2.Zero;
        }

        public static Vector2 GetRightThumbAny(this InputManager input, float deadZone)
        {
            int totalCount = 0;
            Vector2 totalMovement = Vector2.Zero;
            for (int i = 0; i < input.GamePadCount; i++)
            {
                var rightVector = input.GetGamePad(i).State.RightThumb;
                if (rightVector.Length() >= deadZone)
                {
                    totalCount++;
                    totalMovement += rightVector;
                }
            }

            return (totalCount > 1) ? (totalMovement / totalCount) : totalMovement;
        }

        public static float GetLeftTrigger(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.LeftTrigger : 0.0f;
        }

        public static float GetLeftTriggerAny(this InputManager input, float deadZone)
        {
            int totalCount = 0;
            float totalInput = 0;
            for (int i = 0; i < input.GamePadCount; i++)
            {
                float triggerValue = input.GetGamePad(i).State.LeftTrigger;
                if (triggerValue >= deadZone)
                {
                    totalCount++;
                    totalInput += triggerValue;
                }
            }

            return (totalCount > 1) ? (totalInput / totalCount) : totalInput;
        }

        public static float GetRightTrigger(this InputManager input, int index)
        {
            return input.GamePadCount >= index ? input.GetGamePad(index).State.RightTrigger : 0.0f;
        }

        public static float GetRightTriggerAny(this InputManager input, float deadZone)
        {
            int totalCount = 0;
            float totalInput = 0;
            for (int i = 0; i < input.GamePadCount; i++)
            {
                float triggerValue = input.GetGamePad(i).State.RightTrigger;
                if (triggerValue >= deadZone)
                {
                    totalCount++;
                    totalInput += triggerValue;
                }
            }

            return (totalCount > 1) ? (totalInput / totalCount) : totalInput;
        }
    }
}
