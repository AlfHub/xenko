﻿// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using Color = SiliconStudio.Core.Mathematics.Color;

namespace SiliconStudio.Xenko.Profiling
{
    public enum GameProfilingSorting
    {
        [Display("Total Time")]
        ByTime,

        [Display("Key Name")]
        ByName
    }

    public enum GameProfilingResults
    {
        [Display("FPS")]
        Fps,

        [Display("CPU Profiling Events")]
        CpuEvents,

        [Display("GPU Profiling Events")]
        GpuEvents,
    }

    public class GameProfilingSystem : GameSystemBase
    {
        private readonly Point textDrawStartOffset = new Point(5, 10);
        private const int TextRowHeight = 16;
        private const int TopRowHeight = TextRowHeight + 2;

        private readonly GcProfiling gcProfiler;

        private readonly StringBuilder gcMemoryStringBuilder = new StringBuilder();
        private string gcMemoryString = "";
        private readonly string gcMemoryStringBase;

        private readonly StringBuilder gcCollectionsStringBuilder = new StringBuilder();
        private string gcCollectionsString = "";
        private readonly string gcCollectionsStringBase;

        private readonly StringBuilder fpsStatStringBuilder = new StringBuilder();
        private string fpsStatString = "";

        private readonly StringBuilder gpuGeneralInfoStringBuilder = new StringBuilder();
        private string gpuGeneralInfoString = "";

        private readonly StringBuilder gpuInfoStringBuilder = new StringBuilder();
        private string gpuInfoString = "";

        private readonly StringBuilder profilersStringBuilder = new StringBuilder();
        private string profilersString = "";

        private FastTextRenderer fastTextRenderer;

        private readonly object stringLock = new object();

        private Color4 textColor = Color.LightGreen;

        private PresentInterval userPresentInterval = PresentInterval.Default;  
        
        private int lastFrame = -1;

        private float viewportHeight = 1000;

        private uint numberOfPages;

        private uint trianglesCount;
        private uint drawCallsCount;

        private struct ProfilingResult : IComparer<ProfilingResult>
        {
            public long AccumulatedTime;
            public long MinTime;
            public long MaxTime;
            public int Count;
            public int MarkCount;
            public ProfilingEvent? Event;

            public ProfilingCustomValue? Custom0;
            public ProfilingCustomValue? Custom1;
            public ProfilingCustomValue? Custom2;
            public ProfilingCustomValue? Custom3;

            public int Compare(ProfilingResult x, ProfilingResult y)
            {
                return Math.Sign(x.AccumulatedTime - y.AccumulatedTime);
            }
        }

        private readonly List<ProfilingResult> profilingResults = new List<ProfilingResult>();
        private readonly Dictionary<ProfilingKey, ProfilingResult> profilingResultsDictionary = new Dictionary<ProfilingKey, ProfilingResult>();

        public GameProfilingSystem(IServiceRegistry registry) : base(registry)
        {
            DrawOrder = 0xffffff;

            gcProfiler = new GcProfiling();        

            gcMemoryStringBase =      "Allocated Memory> Total: {0:0.00}[MB] Peak: {1:0.00}[MB] Last allocations: {2:0.00}[KB]";
            gcCollectionsStringBase = "Garbage Collections> Gen0: {0}, Gen1: {1}, Gen2: {2}"; 
        }

        private readonly Stopwatch dumpTiming = Stopwatch.StartNew();

        private Task stringBuilderTask;
        private Size2 renderTargetSize;

        public override void Update(GameTime gameTime)
        {
            if (dumpTiming.ElapsedMilliseconds < RefreshTime || FilteringMode == GameProfilingResults.GpuEvents)
                return;
            
            dumpTiming.Restart();

            if(stringBuilderTask != null && !stringBuilderTask.IsCompleted) return;

            stringBuilderTask = new Task(UpdateProfilingStrings);
            stringBuilderTask.Start();
        }

        private void UpdateProfilingStrings()
        {
            profilersStringBuilder.Clear();
            fpsStatStringBuilder.Clear();
            gpuInfoStringBuilder.Clear();
            gpuGeneralInfoStringBuilder.Clear();

            //Advance any profiler that needs it
            gcProfiler.Tick();

            // calculate elaspsed frames
            var newDraw = Game.DrawTime.FrameCount;
            var elapsedFrames = newDraw - lastFrame;
            lastFrame = newDraw;
            
            // Get events from the profiler ( this will also clean up the profiler )
            var events = Profiler.GetEvents(FilteringMode == GameProfilingResults.CpuEvents? ProfilingEventType.CpuProfilingEvent : ProfilingEventType.GpuProfilingEvent);

            if(FilteringMode != GameProfilingResults.Fps)
            { 
                var containsMarks = false;
                var tickFrequency = FilteringMode == GameProfilingResults.GpuEvents ? GraphicsDevice.TimestampFrequency : Stopwatch.Frequency;

                //update strings that need update
                foreach (var e in events)
                {
                    //gc profiling is a special case
                    if (e.Key == GcProfiling.GcMemoryKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                    {
                        gcMemoryStringBuilder.Clear();
                        gcMemoryStringBuilder.AppendFormat(gcMemoryStringBase, e.Custom0.Value.LongValue/(float)0x100000, e.Custom2.Value.LongValue/(float)0x100000, e.Custom1.Value.LongValue/1024f);
                        continue;
                    }

                    if (e.Key == GcProfiling.GcCollectionCountKey && e.Custom0.HasValue && e.Custom1.HasValue && e.Custom2.HasValue)
                    {
                        gcCollectionsStringBuilder.Clear();
                        gcCollectionsStringBuilder.AppendFormat(gcCollectionsStringBase, e.Custom0.Value.IntValue, e.Custom1.Value.IntValue, e.Custom2.Value.IntValue);
                        continue;
                    }

                    if (e.Key == GameProfilingKeys.GameDrawFPS && e.Type == ProfilingMessageType.End)
                        continue;

                    ProfilingResult profilingResult;
                    if (!profilingResultsDictionary.TryGetValue(e.Key, out profilingResult))
                    {
                        profilingResult.MinTime = long.MaxValue;
                    }


                    if (e.Type == ProfilingMessageType.End)
                    {
                        ++profilingResult.Count;
                        profilingResult.AccumulatedTime += e.ElapsedTime;

                        if (e.ElapsedTime < profilingResult.MinTime)
                            profilingResult.MinTime = e.ElapsedTime;
                        if (e.ElapsedTime > profilingResult.MaxTime)
                            profilingResult.MaxTime = e.ElapsedTime;

                        profilingResult.Event = e;
                    }
                    else if (e.Type == ProfilingMessageType.Mark)
                    {
                        profilingResult.MarkCount++;
                        containsMarks = true;
                    }

                    if (e.Custom0.HasValue)
                    {
                        profilingResult.Custom0 = e.Custom0.Value;
                    }
                    if (e.Custom1.HasValue)
                    {
                        profilingResult.Custom1 = e.Custom1.Value;
                    }
                    if (e.Custom2.HasValue)
                    {
                        profilingResult.Custom2 = e.Custom2.Value;
                    }
                    if (e.Custom3.HasValue)
                    {
                        profilingResult.Custom3 = e.Custom3.Value;
                    }

                    profilingResultsDictionary[e.Key] = profilingResult;
                }

                profilersStringBuilder.Clear();
                profilingResults.Clear();

                foreach (var profilingResult in profilingResultsDictionary)
                {
                    if (!profilingResult.Value.Event.HasValue) continue;
                    profilingResults.Add(profilingResult.Value);
                }
                profilingResultsDictionary.Clear();
            
                if (SortingMode == GameProfilingSorting.ByTime)
                {
                    profilingResults.Sort((x1, x2) => Math.Sign(x2.AccumulatedTime - x1.AccumulatedTime));
                }
                else
                {
                    // Can't be null because we skip those events without values
                    // ReSharper disable PossibleInvalidOperationException
                    profilingResults.Sort((x1, x2) => string.Compare(x1.Event.Value.Key.Name, x2.Event.Value.Key.Name, StringComparison.Ordinal));
                    // ReSharper restore PossibleInvalidOperationException
                }
            
                var availableDisplayHeight = viewportHeight - 2 * TextRowHeight - 3 * TopRowHeight;
                var elementsPerPage = (int)Math.Floor(availableDisplayHeight / TextRowHeight);
                numberOfPages = (uint) Math.Ceiling(profilingResults.Count / (float) elementsPerPage);
                CurrentResultPage = Math.Min(CurrentResultPage, numberOfPages);
            
                profilersStringBuilder.AppendFormat("TOTAL     | AVG/CALL  | MIN/CALL  | MAX/CALL  | CALLS | ");
                if (containsMarks)
                    profilersStringBuilder.AppendFormat("MARKS | ");
                profilersStringBuilder.AppendFormat("PROFILING KEY / EXTRA INFO\n");

                for (int i = 0; i < Math.Min(profilingResults.Count - (CurrentResultPage-1) * elementsPerPage, elementsPerPage); i++)
                {
                    AppendEvent(profilingResults[((int)CurrentResultPage-1)*elementsPerPage + i], elapsedFrames, tickFrequency, containsMarks);
                }
                profilingResults.Clear();

                if(numberOfPages > 1)
                    profilersStringBuilder.AppendFormat("PAGE {0} OF {1}", CurrentResultPage, numberOfPages);

                gpuInfoStringBuilder.Clear();
                gpuInfoStringBuilder.AppendFormat("Drawn triangles: {0:0.0}k, Draw calls: {1}, Buffer memory: {2:0.00}[MB], Texture memory: {3:0.00}[MB]", trianglesCount/1000f, drawCallsCount, GraphicsDevice.BuffersMemory, GraphicsDevice.TextureMemory);

                gpuGeneralInfoStringBuilder.Clear();
                gpuGeneralInfoStringBuilder.AppendFormat("Device: {0}, Platform: {1}, Profile: {2}, Resolution: {3}", GraphicsDevice.Adapter.Description, GraphicsDevice.Platform, GraphicsDevice.ShaderProfile, renderTargetSize);
            }

            fpsStatStringBuilder.Clear();
            fpsStatStringBuilder.AppendFormat("Displaying: {0}, Frame: {1}, Update: {2:0.00}ms, Draw: {3:0.00}ms, FPS: {4:0.00}", FilteringMode, Game.DrawTime.FrameCount, Game.UpdateTime.TimePerFrame.TotalMilliseconds, Game.DrawTime.TimePerFrame.TotalMilliseconds, Game.DrawTime.FramePerSecond);

            lock (stringLock)
            {
                gcCollectionsString = gcCollectionsStringBuilder.ToString();
                gcMemoryString = gcMemoryStringBuilder.ToString();
                profilersString = profilersStringBuilder.ToString();
                fpsStatString = fpsStatStringBuilder.ToString();
                gpuInfoString = gpuInfoStringBuilder.ToString();
                gpuGeneralInfoString = gpuGeneralInfoStringBuilder.ToString();
            }
        }

        private void AppendEvent(ProfilingResult profilingResult, int elapsedFrames, long tickFrequency, bool displayMarkCount)
        {
            var profilingEvent = profilingResult.Event.Value;

            Profiler.AppendTime(profilersStringBuilder, profilingResult.AccumulatedTime / elapsedFrames, tickFrequency);
            profilersStringBuilder.Append(" | ");
            Profiler.AppendTime(profilersStringBuilder, profilingResult.AccumulatedTime / profilingResult.Count, tickFrequency);
            profilersStringBuilder.Append(" | ");
            Profiler.AppendTime(profilersStringBuilder, profilingResult.MinTime, tickFrequency);
            profilersStringBuilder.Append(" | ");
            Profiler.AppendTime(profilersStringBuilder, profilingResult.MaxTime, tickFrequency);
            profilersStringBuilder.Append(" | ");
            profilersStringBuilder.AppendFormat("{0:00.00}", profilingResult.Count / (double)elapsedFrames);
            profilersStringBuilder.Append(" | ");

            if (displayMarkCount)
            {
                profilersStringBuilder.AppendFormat("{0:00.00}", profilingResult.MarkCount / (double)elapsedFrames);
                profilersStringBuilder.Append(" | ");
            }

            profilersStringBuilder.Append(profilingEvent.Key);
            // ReSharper disable once ReplaceWithStringIsNullOrEmpty
            // This was creating memory allocation (GetEnumerable())
            if (profilingEvent.Text != null && profilingEvent.Text != "")
            {
                profilersStringBuilder.Append(" / ");
                profilersStringBuilder.AppendFormat(profilingEvent.Text, GetValue(profilingResult.Custom0), GetValue(profilingResult.Custom1), GetValue(profilingResult.Custom2), GetValue(profilingResult.Custom3));
            }

            profilersStringBuilder.Append("\n");
        }

        private static string GetValue(ProfilingCustomValue? value)
        {
            if (!value.HasValue) return "";

            if (value.Value.ValueType == typeof(int))
            {
                return value.Value.IntValue.ToString();
            }
            if (value.Value.ValueType == typeof(float))
            {
                return value.Value.FloatValue.ToString(CultureInfo.InvariantCulture);
            }
            if (value.Value.ValueType == typeof(long))
            {
                return value.Value.LongValue.ToString();
            }
            if (value.Value.ValueType == typeof(double))
            {
                return value.Value.DoubleValue.ToString(CultureInfo.InvariantCulture);
            }

            return "";
        }

        protected override void Destroy()
        {
            Enabled = false;
            Visible = false;

            if (stringBuilderTask != null && !stringBuilderTask.IsCompleted)
            {
                stringBuilderTask.Wait();
            }

            gcProfiler.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            // copy those values before fast text render not to influence the game stats
            drawCallsCount = GraphicsDevice.FrameDrawCalls;
            trianglesCount = GraphicsDevice.FrameTriangleCount;

            if (dumpTiming.ElapsedMilliseconds > RefreshTime && FilteringMode == GameProfilingResults.GpuEvents)
            {
                dumpTiming.Restart();

                renderTargetSize = new Size2(Game.GraphicsContext.CommandList.RenderTarget.Width, Game.GraphicsContext.CommandList.RenderTarget.Height);

                if (stringBuilderTask == null || stringBuilderTask.IsCompleted)
                {
                    stringBuilderTask = new Task(UpdateProfilingStrings);
                    stringBuilderTask.Start();
                }
            }

            viewportHeight = Game.GraphicsContext.CommandList.Viewport.Height;

            if (fastTextRenderer == null)
            {
                fastTextRenderer = new FastTextRenderer
                {
                    DebugSpriteFont = Content.Load<Texture>("XenkoDebugSpriteFont"),
                    TextColor = TextColor
                }.Initialize(Game.GraphicsContext);
            }

            // TODO GRAPHICS REFACTOR where to get command list from?
            Game.GraphicsContext.CommandList.SetRenderTargetAndViewport(null, Game.GraphicsDevice.Presenter.BackBuffer);
            fastTextRenderer.Begin(Game.GraphicsContext);
            lock (stringLock)
            {
                var currentHeight = textDrawStartOffset.Y;
                fastTextRenderer.DrawString(Game.GraphicsContext, fpsStatString, textDrawStartOffset.X, currentHeight);
                currentHeight += TopRowHeight;

                if (FilteringMode == GameProfilingResults.CpuEvents)
                {
                    fastTextRenderer.DrawString(Game.GraphicsContext, gcMemoryString, textDrawStartOffset.X, currentHeight);
                    currentHeight += TopRowHeight;
                    fastTextRenderer.DrawString(Game.GraphicsContext, gcCollectionsString, textDrawStartOffset.X, currentHeight);
                    currentHeight += TopRowHeight;
                }
                else if(FilteringMode == GameProfilingResults.GpuEvents)
                {
                    fastTextRenderer.DrawString(Game.GraphicsContext, gpuGeneralInfoString, textDrawStartOffset.X, currentHeight);
                    currentHeight += TopRowHeight;
                    fastTextRenderer.DrawString(Game.GraphicsContext, gpuInfoString, textDrawStartOffset.X, currentHeight);
                    currentHeight += TopRowHeight;
                }

                if(FilteringMode != GameProfilingResults.Fps)
                    fastTextRenderer.DrawString(Game.GraphicsContext, profilersString, textDrawStartOffset.X, currentHeight);
            }

            fastTextRenderer.End(Game.GraphicsContext);
        }

        /// <summary>
        /// Enables the profiling system drawing.
        /// </summary>
        /// <param name="excludeKeys">If true the keys specified after are excluded from rendering, if false they will be exclusively included.</param>
        /// <param name="keys">The keys to exclude or include.</param>
        public void EnableProfiling(bool excludeKeys = false, params ProfilingKey[] keys)
        {
            Enabled = true;
            Visible = true;

            // Backup current PresentInterval state
            userPresentInterval = GraphicsDevice.Presenter.PresentInterval;

            // Disable VSync (otherwise GPU results might be incorrect)
            GraphicsDevice.Presenter.PresentInterval = PresentInterval.Immediate;

            if (keys.Length == 0)
            {
                Profiler.EnableAll();
            }
            else
            {
                if (excludeKeys)
                {
                    Profiler.EnableAll();
                    foreach (var profilingKey in keys)
                    {
                        Profiler.Disable(profilingKey);
                    }
                }
                else
                {
                    foreach (var profilingKey in keys)
                    {
                        Profiler.Enable(profilingKey);
                    }
                }              
            }

            gcProfiler.Enable();
        }

        /// <summary>
        /// Disables the profiling system drawing.
        /// </summary>
        public void DisableProfiling()
        {
            Enabled = false;
            Visible = false;

            // Restore previous PresentInterval state
            GraphicsDevice.Presenter.PresentInterval = userPresentInterval;
            userPresentInterval = PresentInterval.Default;

            Profiler.DisableAll();
            gcProfiler.Disable();
        }

        /// <summary>
        /// Sets or gets the color to use when drawing the profiling system fonts.
        /// </summary>
        public Color4 TextColor
        {
            get => textColor;
            set
            {
                textColor = value;
                if (fastTextRenderer != null)
                    fastTextRenderer.TextColor = value;
            }
        }

        /// <summary>
        /// Sets or gets the way the printed information will be sorted.
        /// </summary>
        public GameProfilingSorting SortingMode { get; set; } = GameProfilingSorting.ByTime;

        /// <summary>
        /// Sets or gets which data should be displayed on screen.
        /// </summary>
        public GameProfilingResults FilteringMode { get; set; } = GameProfilingResults.Fps;

        /// <summary>
        /// Sets or gets the refreshing time of the profiling information in milliseconds.
        /// </summary>
        public double RefreshTime { get; set; } = 500;

        /// <summary>
        /// Sets or gets the profiling result page to display.
        /// </summary>
        public uint CurrentResultPage { get ; set; } = 1;
    }
}
