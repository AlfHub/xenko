﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using UIKit;

namespace SiliconStudio.Xenko.Input
{
    public class PointeriOS : PointerDeviceBase
    {
        private XenkoGameController gameController;
        private iOSWindow uiControl;
        private Dictionary<int, int> touchFingerIndexMap = new Dictionary<int, int>();
        private int touchCounter = 0;

        public PointeriOS(iOSWindow uiControl, XenkoGameController gameController)
        {
            this.uiControl = uiControl;
            this.gameController = gameController;
            var window = uiControl.MainWindow;
            window.UserInteractionEnabled = true;
            window.MultipleTouchEnabled = true;
            uiControl.GameView.MultipleTouchEnabled = true;
            gameController.TouchesBeganDelegate += OnTouch;
            gameController.TouchesMovedDelegate += OnTouch;
            gameController.TouchesEndedDelegate += OnTouch;
            gameController.TouchesCancelledDelegate += OnTouch;
            uiControl.GameView.Resize += OnResize;

            OnResize(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();
            gameController.TouchesBeganDelegate -= OnTouch;
            gameController.TouchesMovedDelegate -= OnTouch;
            gameController.TouchesEndedDelegate -= OnTouch;
            gameController.TouchesCancelledDelegate -= OnTouch;
            uiControl.GameView.Resize -= OnResize;
        }
        
        public override string DeviceName => "iOS Pointer";
        public override Guid Id => new Guid("6fa378ee-1ffe-41c1-947a-b425adcd5258");
        public override PointerType Type => PointerType.Touch;

        private void OnTouch(NSSet touchesSet, UIEvent evt)
        {
            var touches = touchesSet.ToArray<UITouch>();

            if (touches != null)
            {
                // Convert touches to pointer events
                foreach (var uitouch in touches)
                {
                    var pointerEvent = new PointerInputEvent();
                    var touchId = uitouch.Handle.ToInt32();
                    

                    pointerEvent.Position = CGPointToVector2(uitouch.LocationInView(uiControl.GameView))*InverseSurfaceSize;
                    switch (uitouch.Phase)
                    {
                        case UITouchPhase.Began:
                            pointerEvent.Type = InputEventType.Down;
                            break;
                        case UITouchPhase.Moved:
                        case UITouchPhase.Stationary:
                            pointerEvent.Type = InputEventType.Move;
                            break;
                        case UITouchPhase.Ended:
                        case UITouchPhase.Cancelled:
                            pointerEvent.Type = InputEventType.Up;
                            break;
                        default:
                            throw new ArgumentException("Got an invalid Touch event in GetState");
                    }

                    // Assign finger index (starting at 0) to touch ID
                    int touchFingerIndex = 0;
                    if (pointerEvent.Type == InputEventType.Down)
                    {
                        touchFingerIndex = touchCounter++;
                        touchFingerIndexMap.Add(touchId, touchFingerIndex);
                    }
                    else
                    {
                        touchFingerIndex = touchFingerIndexMap[touchId];
                    }

                    // Remove index
                    if (pointerEvent.Type == InputEventType.Up)
                    {
                        touchFingerIndexMap.Remove(touchId);
                        touchCounter = 0; // Reset touch counter

                        // Recalculate next finger index
                        if (touchFingerIndexMap.Count > 0)
                        {
                            touchFingerIndexMap.ForEach(pair => touchCounter = Math.Max(touchCounter, pair.Value));
                            touchCounter++; // next
                        }
                    }

                    pointerEvent.Id = touchFingerIndex;
                    pointerInputEvents.Add(pointerEvent);
                }
            }
        }
        
        private void OnResize(object sender, EventArgs eventArgs)
        {
            SetSurfaceSize(new Vector2(
                (float)uiControl.GameView.Frame.Width,
                (float)uiControl.GameView.Frame.Height));
        }

        private Vector2 CGPointToVector2(CGPoint point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
    }
}
#endif