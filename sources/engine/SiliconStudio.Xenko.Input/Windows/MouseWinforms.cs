﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using Point = System.Drawing.Point;

namespace SiliconStudio.Xenko.Input
{
    internal class MouseWinforms : MouseDeviceBase, IDisposable
    {
        private readonly GameBase game;
        private readonly Control uiControl;
        private bool isPositionLocked;
        private bool wasVisibleBeforeCapture;
        private Point capturedPosition;

        // Stored position for SetPosition
        private Point targetPosition;
        private bool shouldSetPosition;

        public MouseWinforms(GameBase game, Control uiControl)
        {
            this.game = game;
            this.uiControl = uiControl;
            
            uiControl.MouseMove += OnMouseMove;
            uiControl.MouseDown += OnMouseDown;
            uiControl.MouseUp += OnMouseUp;
            uiControl.MouseWheel += OnMouseWheelEvent;
            uiControl.MouseCaptureChanged += OnLostMouseCapture;
            uiControl.SizeChanged += OnSizeChanged;

            OnSizeChanged(this, null);
        }

        public void Dispose()
        {
            uiControl.MouseMove -= OnMouseMove;
            uiControl.MouseDown -= OnMouseDown;
            uiControl.MouseUp -= OnMouseUp;
            uiControl.MouseWheel -= OnMouseWheelEvent;
            uiControl.MouseCaptureChanged -= OnLostMouseCapture;
            uiControl.SizeChanged -= OnSizeChanged;
        }

        public override string Name => "Windows Mouse";
        public override Guid Id => new Guid("699e35c5-c363-4bb0-8e8b-0474ea1a5cf1");
        public override bool IsPositionLocked => isPositionLocked;
        public override PointerType Type => PointerType.Mouse;

        public override void Update(List<InputEvent> inputEvents)
        {
            base.Update(inputEvents);

            // Set mouse position
            if (shouldSetPosition)
            {
                Cursor.Position = targetPosition;
                shouldSetPosition = false;
            }
        }

        public override void SetPosition(Vector2 normalizedPosition)
        {
            Vector2 position = normalizedPosition*SurfaceSize;

            // Store setting of mouse position since it will keep the message loop goining infinitely otherwise
            var targetPoint = new Point((int)position.X, (int)position.Y);
            targetPosition = uiControl.PointToScreen(targetPoint);
            shouldSetPosition = true;
        }

        public override void LockPosition(bool forceCenter = false)
        {
            if (!isPositionLocked)
            {
                wasVisibleBeforeCapture = game.IsMouseVisible;
                game.IsMouseVisible = false;
                if (forceCenter)
                {
                    capturedPosition = uiControl.PointToScreen(new Point(uiControl.ClientSize.Width/2, uiControl.ClientSize.Height/2));
                }
                capturedPosition = Cursor.Position;
                isPositionLocked = true;
            }
        }

        public override void UnlockPosition()
        {
            if (isPositionLocked)
            {
                isPositionLocked = false;
                capturedPosition = System.Drawing.Point.Empty;
                game.IsMouseVisible = wasVisibleBeforeCapture;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isPositionLocked)
            {
                // Register mouse delta and reset
                HandleMouseDelta(new Vector2(Cursor.Position.X - capturedPosition.X, Cursor.Position.Y - capturedPosition.Y));
                targetPosition = capturedPosition;
                shouldSetPosition = true;
            }
            else
            {
                HandleMove(new Vector2(e.X, e.Y));
            }
        }

        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            SetSurfaceSize(new Vector2(uiControl.ClientSize.Width, uiControl.ClientSize.Height));
        }

        private void OnMouseWheelEvent(object sender, MouseEventArgs mouseEventArgs)
        {
            // The mouse wheel event are still received even when the mouse cursor is out of the control boundaries. Discard the event in this case.
            if (!uiControl.ClientRectangle.Contains(uiControl.PointToClient(Control.MousePosition)))
                return;

            HandleMouseWheel((float)mouseEventArgs.Delta / (float)SystemInformation.MouseWheelScrollDelta);
        }

        private void OnMouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            HandleButtonUp(ConvertMouseButton(mouseEventArgs.Button));
        }

        private void OnMouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            uiControl.Focus();
            HandleButtonDown(ConvertMouseButton(mouseEventArgs.Button));
        }
        private void OnLostMouseCapture(object sender, EventArgs args)
        {
            // On windows forms, the controls capture of the mouse button events at the first button pressed and release them at the first button released.
            // This has for consequence that all up-events of button simultaneously pressed are lost after the release of first button (if outside of the window).
            // This function fixes the problem by forcing the mouse event capture if any mouse buttons are still down at the first button release.
            //if(DownButtons.Count > 0)
            //{
            //    uiControl.Capture = true;
            //}
            var buttonsToRelease = DownButtons.ToArray();
            foreach (var button in buttonsToRelease)
            {
                HandleButtonUp(button);
            }
        }

        private static MouseButton ConvertMouseButton(MouseButtons mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButtons.Left:
                    return MouseButton.Left;
                case MouseButtons.Right:
                    return MouseButton.Right;
                case MouseButtons.Middle:
                    return MouseButton.Middle;
                case MouseButtons.XButton1:
                    return MouseButton.Extended1;
                case MouseButtons.XButton2:
                    return MouseButton.Extended2;
            }
            return (MouseButton)(-1);
        }
    }
}
#endif