﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal sealed class GestureRecognizerLongPress : GestureRecognizer
    {
        private GestureConfigLongPress ConfigLongPress { get { return (GestureConfigLongPress)Config; } }

        protected override int NbOfFingerOnScreen
        {
            get { return FingerIdToBeginPositions.Count; }
        }

        public GestureRecognizerLongPress(GestureConfigLongPress config, float screenRatio)
            :base(config, screenRatio)
        {
        }

        protected override void ProcessPointerEventsImpl(TimeSpan deltaTime, List<PointerEvent> events)
        {
            AnalysePointerEvents(events);

            if (HasGestureStarted && ElapsedSinceBeginning >= ConfigLongPress.RequiredPressTime)
            {
                var avgPosition = ComputeMeanPosition(FingerIdToBeginPositions.Values);
                CurrentGestureEvents.Add(new GestureEventLongPress(ConfigLongPress.RequiredNumberOfFingers, ElapsedSinceBeginning, NormalizeVector(avgPosition)));
                HasGestureStarted = false;
            }
        }

        protected override void ProcessDownEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions[id] = pos;
            HasGestureStarted = (NbOfFingerOnScreen == ConfigLongPress.RequiredNumberOfFingers);
        }

        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            foreach (var id in fingerIdsToMovePos.Keys)
            {
                // Only process if a finger is down
                if (!FingerIdToBeginPositions.ContainsKey(id))
                    continue;

                var dist = (fingerIdsToMovePos[id] - FingerIdToBeginPositions[id]).Length();
                if (dist > ConfigLongPress.MaximumTranslationDistance)
                    HasGestureStarted = false;
            }
        }

        protected override void ProcessUpEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions.Remove(id);
            HasGestureStarted = (NbOfFingerOnScreen == ConfigLongPress.RequiredNumberOfFingers);
        }
    }
}