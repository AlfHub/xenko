﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using SDL2;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.SDL;

namespace SiliconStudio.Xenko.Input
{
    public class MouseSDL : MouseDeviceBase, IDisposable
    {
        private bool isMousePositionLocked;
        private bool wasMouseVisibleBeforeCapture;
        private GameBase game;
        private Window uiControl;
        private Point relativeCapturedPosition;

        public MouseSDL(GameBase game, Window uiControl)
        {
            this.game = game;
            this.uiControl = uiControl;
            
            uiControl.MouseMoveActions += OnMouseMoveEvent;
            uiControl.PointerButtonPressActions += OnMouseInputEvent;
            uiControl.PointerButtonReleaseActions += OnMouseInputEvent;
            uiControl.MouseWheelActions += OnMouseWheelEvent;
            uiControl.ResizeEndActions += OnSizeChanged;
            OnSizeChanged(new SDL.SDL_WindowEvent());
        }
        
        public void Dispose()
        {
            uiControl.MouseMoveActions -= OnMouseMoveEvent;
            uiControl.PointerButtonPressActions -= OnMouseInputEvent;
            uiControl.PointerButtonReleaseActions -= OnMouseInputEvent;
            uiControl.MouseWheelActions -= OnMouseWheelEvent;
            uiControl.ResizeEndActions -= OnSizeChanged;
        }

        public override string Name => "SDL Mouse";
        public override Guid Id => new Guid("0ccaf48e-e371-4b34-b6bb-a3720f6742a8");
        public override bool IsPositionLocked => isMousePositionLocked;

        public override void LockPosition(bool forceCenter = false)
        {
            if (!IsPositionLocked)
            {
                wasMouseVisibleBeforeCapture = game.IsMouseVisible;
                game.IsMouseVisible = false;
                if (forceCenter)
                {
                    SetPosition(new Vector2(0.5f, 0.5f));
                }
                relativeCapturedPosition = uiControl.RelativeCursorPosition;
                isMousePositionLocked = true;
            }
        }

        public override void UnlockPosition()
        {
            if (IsPositionLocked)
            {
                isMousePositionLocked = false;
                relativeCapturedPosition = Point.Zero;
                game.IsMouseVisible = wasMouseVisibleBeforeCapture;
            }
        }

        public override void SetPosition(Vector2 normalizedPosition)
        {
            Vector2 position = normalizedPosition*SurfaceSize;
            Cursor.Position = new Point((int)position.X, (int)position.Y);
        }
        
        private void OnSizeChanged(SDL.SDL_WindowEvent eventArgs)
        {
            SetSurfaceSize(new Vector2(uiControl.ClientSize.Width, uiControl.ClientSize.Height));
        }

        private void OnMouseWheelEvent(SDL.SDL_MouseWheelEvent sdlMouseWheelEvent)
        {
            var flip = (sdlMouseWheelEvent.direction == (uint)SDL.SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED) ? -1 : 1;
            HandleMouseWheel(sdlMouseWheelEvent.y * flip);
        }

        private void OnMouseInputEvent(SDL.SDL_MouseButtonEvent e)
        {
            MouseButton button = ConvertMouseButton(e.button);
            if(e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
                HandleButtonDown(button);
            else
                HandleButtonUp(button);
        }

        private void OnMouseMoveEvent(SDL.SDL_MouseMotionEvent e)
        {
            if (IsPositionLocked)
            {
                HandleMouseDelta(new Vector2(e.x - relativeCapturedPosition.X, e.y - relativeCapturedPosition.Y));

                // Restore position to prevent mouse from going out of the window where we would not get
                // mouse move event.
                uiControl.RelativeCursorPosition = relativeCapturedPosition;
            }
            else
            {
                HandleMove(new Vector2(e.x, e.y));
            }
        }

        private static MouseButton ConvertMouseButton(uint mouseButton)
        {
            switch (mouseButton)
            {
                case SDL.SDL_BUTTON_LEFT:
                    return MouseButton.Left;
                case SDL.SDL_BUTTON_RIGHT:
                    return MouseButton.Right;
                case SDL.SDL_BUTTON_MIDDLE:
                    return MouseButton.Middle;
                case SDL.SDL_BUTTON_X1:
                    return MouseButton.Extended1;
                case SDL.SDL_BUTTON_X2:
                    return MouseButton.Extended2;
            }
            return (MouseButton)(-1);
        }
    }
}
#endif