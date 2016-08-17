﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SiliconStudio.Presentation.Behaviors;
using SiliconStudio.Presentation.Extensions;
using GraphX;
using System.Diagnostics;
using System.Windows.Interactivity;
using GraphX.Controls;
using System.Collections.Generic;
using System.Collections;
using GraphX.Models;
using System.Windows.Shapes;

namespace SiliconStudio.Presentation.Graph.Behaviors
{
    public class LinkPreviewBehavior : DeferredBehaviorBase<GraphAreaBase> //Behavior<GraphAreaBase> 
    {
        /// <summary>
        /// 
        /// </summary>
        public sealed class LinkPreviewAdorner : Adorner
        {
            private readonly Pen pen_;
            private Point start_ = new Point();
            private Point end_ = new Point();

            public LinkPreviewAdorner(UIElement parent)
                : base(parent)
            {   //

                Brush brush = new SolidColorBrush(Colors.White);
                brush.Opacity = 0.5;

                pen_ = new Pen(brush, 1.0);
                pen_.DashStyle = new DashStyle(new double[] { 3, 2, 3, 2 }, 0);
                pen_.DashCap = PenLineCap.Flat;
                pen_.StartLineCap = PenLineCap.Round;
                pen_.EndLineCap = PenLineCap.Round;                
                pen_.Thickness = 4;               
                pen_.Freeze();
            }

            public Point Start {
                get { return start_; }
                set
                {
                    start_ = value;
                    InvalidateVisual();
                }
            }

            public Point End
            {
                get { return end_; }
                set
                {
                    end_ = value;
                    InvalidateVisual();
                }
            }



            /// <summary>
            /// Participates in rendering operations that are directed by the layout system.
            /// </summary>
            /// <param name="drawingContext">The drawing instructions.</param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (IsEnabled)
                {
                    //
                    //Debug.WriteLine("drawing...");
                    drawingContext.DrawLine(pen_, start_, end_);
                }
            }
        }

        #region Static Members
        public static LinkPreviewAdorner LinkPreview;
        #endregion

        #region Members
        private GraphAreaBase graph_area_;        
        #endregion

        #region
        protected override void OnAttachedAndLoaded()
        {
            graph_area_ = AssociatedObject;            

            Register();
        }

        protected override void OnDetachingAndUnloaded()
        {
            Unregister();
        }

        private void Register()
        {
            // Create it!
            if (LinkPreview == null)
            {
                var adornLayer = AdornerLayer.GetAdornerLayer(graph_area_);
                if (adornLayer == null) 
                {
                    Debug.Write("Bad");
                }                
                LinkPreview = new LinkPreviewAdorner(graph_area_);
                LinkPreview.IsHitTestVisible = false;
                adornLayer.Add(LinkPreview);
            }
        }

        private void Unregister()
        {
            // Destroy it!
            if (LinkPreview == null)
            {
                var adornLayer = AdornerLayer.GetAdornerLayer(graph_area_);
                if (adornLayer == null) 
                {
                    Debug.Write("Bad");
                }

                adornLayer.Remove(LinkPreview);
                LinkPreview = null;
            }
        }
        #endregion
    }
}
