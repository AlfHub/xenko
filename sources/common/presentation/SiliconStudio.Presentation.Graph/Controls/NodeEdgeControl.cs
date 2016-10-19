﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


using GraphX;
using GraphX.Controls.Models;
using SiliconStudio.Presentation.Graph.Helper;
using SiliconStudio.Presentation.Extensions;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Graph.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [TemplatePart(Name = "PART_linkItemsControl", Type = typeof(ItemsControl))]
    public class NodeEdgeControl : EdgeControl, INotifyPropertyChanged
    {
        private Path path;
        private Path arrow;

        #region Dependency Properties
        public static readonly DependencyProperty SourceSlotProperty = DependencyProperty.Register("SourceSlot", typeof(object), typeof(NodeEdgeControl));
        public static readonly DependencyProperty TargetSlotProperty = DependencyProperty.Register("TargetSlot", typeof(object), typeof(NodeEdgeControl));
        public static readonly DependencyProperty LinkStrokeProperty = DependencyProperty.Register("LinkStroke", typeof(Brush), typeof(NodeEdgeControl), new PropertyMetadata(Brushes.LightGray));
        public static readonly DependencyProperty LinkStrokeThicknessProperty = DependencyProperty.Register("LinkStrokeThickness", typeof(double), typeof(NodeEdgeControl), new PropertyMetadata(5.0));
        public static readonly DependencyProperty MouseOverLinkStrokeProperty = DependencyProperty.Register("MouseOverLinkStroke", typeof(Brush), typeof(NodeEdgeControl), new PropertyMetadata(Brushes.Green));
        public static readonly DependencyProperty SelectedLinkStrokeProperty = DependencyProperty.Register("SelectedLinkStroke", typeof(Brush), typeof(NodeEdgeControl), new PropertyMetadata(Brushes.LightGreen));
        #endregion

        #region Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="edge"></param>
        /// <param name="showLabels"></param>
        /// <param name="showArrows"></param>
        public NodeEdgeControl(VertexControl source, VertexControl target, object edge, bool showLabels = false, bool showArrows = true)
            : base(source, target, edge, showLabels, showArrows)
        {
            this.Loaded += OnLoaded;
        }

        /// <summary>
        /// 
        /// </summary>
        public NodeEdgeControl()
            : base()
        {
            this.Loaded += OnLoaded;
        }
        #endregion

        #region Event Handlers

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Template != null)
            {
                path = Template.FindName("PART_edgePath", this) as Path;
                arrow = Template.FindName("PART_edgeArrowPath", this) as Path;

                //
                UpdateEdge();                               
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLinkMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (RootArea != null && Visibility == Visibility.Visible)
            {
                (RootArea as NodeGraphArea).OnLinkSelected(sender as FrameworkElement);
            }
            e.Handled = true;
        }
        #endregion

        #region Links & Path Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateLabel"></param>
        public override void UpdateEdge(bool updateLabel = true)
        {
            // TODO Need to let the styling come through for the paths!

            base.UpdateEdge(updateLabel);

            // Template not loaded yet?
            if (path == null)
                return;

            var bezier = new BezierSegment();
            var geometry = new PathGeometry();
            var figure = new PathFigure();

            figure.Segments.Add(bezier);
            geometry.Figures.Add(figure);                

            // Find the output slot 
            DependencyObject slot = null;
            if (SourceSlot != null && (Source as NodeVertexControl).Connectors.TryGetValue(SourceSlot, out slot))
            {
                var container = VisualTreeHelper.GetChild(Source, 0) as UIElement;
                var offset = (slot as UIElement).TransformToAncestor(container).Transform(new Point(0, 0));
                var location = Source.GetPosition() + (Vector)offset;                      
                var halfsize = new Vector((double)slot.GetValue(FrameworkElement.WidthProperty) * 0.8,
                                                (double)slot.GetValue(FrameworkElement.HeightProperty) / 2.0);

                figure.SetCurrentValue(PathFigure.StartPointProperty, location + halfsize);
                //figure.StartPoint = location + halfsize;                        
            }

            // Find input slot
            if (TargetSlot != null && (Target as NodeVertexControl).Connectors.TryGetValue(TargetSlot, out slot))
            {
                var container = VisualTreeHelper.GetChild(Target, 0) as UIElement;
                var offset = (slot as UIElement).TransformToAncestor(container).Transform(new Point(0, 0));
                var location = Target.GetPosition() + (Vector)offset;

                //
                var halfsize = new Vector((double)slot.GetValue(FrameworkElement.WidthProperty)*0.2,
                    (double)slot.GetValue(FrameworkElement.HeightProperty)/2.0);

                bezier.SetCurrentValue(BezierSegment.Point3Property, location + halfsize);
                //bezier.Point3 = location + halfsize;   
            }

            var length = bezier.Point3.X - figure.StartPoint.X;
            var curvature = length * 0.4;

            bezier.SetCurrentValue(BezierSegment.Point1Property, new Point(figure.StartPoint.X + curvature, figure.StartPoint.Y));
            //bezier.Point1 = new Point(figure.StartPoint.X + curvage, figure.StartPoint.Y);
            bezier.SetCurrentValue(BezierSegment.Point2Property, new Point(bezier.Point3.X - curvature, bezier.Point3.Y));
            //bezier.Point2 = new Point(bezier.Point3.X - curvage, bezier.Point3.Y);

            //
            path.Data = geometry;
            if (arrow != null)
                arrow.Data = new PathGeometry { Figures = { GeometryHelper.GenerateOldArrow(bezier.Point2, bezier.Point3) } };

            // TODO Should I be doing this here??? should I be uing setcurrentvalue??
            Visibility = Visibility.Visible;
        }
        #endregion

        #region Properties
        public object SourceSlot { get { return (object)GetValue(SourceSlotProperty); } set { SetValue(SourceSlotProperty, value); } }
        public object TargetSlot { get { return (object)GetValue(TargetSlotProperty); } set { SetValue(TargetSlotProperty, value); } }
        public Brush LinkStroke { get { return (Brush)GetValue(LinkStrokeProperty); } set { SetValue(LinkStrokeProperty, value); } }
        public double LinkStrokeThickness { get { return (double)GetValue(LinkStrokeThicknessProperty); } set { SetValue(LinkStrokeThicknessProperty, value); } }
        public Brush MouseOverLinkStroke { get { return (Brush)GetValue(MouseOverLinkStrokeProperty); } set { SetValue(MouseOverLinkStrokeProperty, value); } }
        public Brush SelectedLinkStroke { get { return (Brush)GetValue(SelectedLinkStrokeProperty); } set { SetValue(SelectedLinkStrokeProperty, value); } }
        internal Path Path => path;
        #endregion

        #region Notify Property Change Method
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
