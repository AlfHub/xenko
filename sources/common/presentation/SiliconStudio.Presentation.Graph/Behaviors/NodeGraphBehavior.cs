﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Markup;

using GraphX;
using GraphX.Controls;
using GraphX.GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;
using GraphX.GraphSharp.Algorithms.Layout;
using QuickGraph;

using SiliconStudio.Presentation.Graph.ViewModel;
using SiliconStudio.Presentation.Graph.Controls;

namespace SiliconStudio.Presentation.Graph.Behaviors
{
    /// <summary>
    /// 
    /// </summary>
    [ContentProperty("ConnectionWrappers")]
    public class NodeGraphBehavior : Behavior<GraphArea<NodeVertex, NodeEdge, BidirectionalGraph<NodeVertex, NodeEdge>>>
    {
        #region ConnectionWrapperData Struct
        /// <summary>
        /// 
        /// </summary>
        struct ConnectionWrapperData
        {
            public NodeGraphBehavior Owner;
            public ConnectionWrapper Wrapper;
            public NodeVertex Vertex;

            public ConnectionWrapperData(NodeGraphBehavior owner, ConnectionWrapper wrapper, NodeVertex vertex)
            {
                Owner = owner;
                Wrapper = wrapper;
                Vertex = vertex;
            }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty VerticesProperty = DependencyProperty.Register("Vertices", typeof(IEnumerable), typeof(NodeGraphBehavior), new PropertyMetadata(null, OnVerticesChanged));
        public static readonly DependencyProperty EdgesProperty = DependencyProperty.Register("Edges", typeof(IEnumerable), typeof(NodeGraphBehavior), new PropertyMetadata(null, OnEdgesChanged));
        #endregion

        #region Static Dependency Property Event Handler
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnVerticesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (NodeGraphBehavior)d;

            var oldList = e.OldValue as INotifyCollectionChanged;
            if (oldList != null) { oldList.CollectionChanged -= behavior.OnVerticesCollectionChanged; }

            var newList = e.NewValue as INotifyCollectionChanged;
            if (newList != null) { newList.CollectionChanged += behavior.OnVerticesCollectionChanged; }

            behavior.Vertices = e.NewValue as IEnumerable;
            behavior.RecreateGraph(behavior);
            behavior.RelayoutGraph();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnEdgesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (NodeGraphBehavior)d;

            var oldList = e.OldValue as INotifyCollectionChanged;
            if (oldList != null) { oldList.CollectionChanged -= behavior.OnEdgesCollectionChanged; }

            var newList = e.NewValue as INotifyCollectionChanged;
            if (newList != null) { newList.CollectionChanged += behavior.OnEdgesCollectionChanged; }

            behavior.Edges = e.NewValue as IEnumerable;
            behavior.RecreateGraph(behavior);
            behavior.RelayoutGraph();
        }
       
        #endregion

        #region Members
        private BidirectionalGraph<NodeVertex, NodeEdge> graph;
        private readonly List<ConnectionWrapper> connection_wrappers_ = new List<ConnectionWrapper>();        
        #endregion

        #region Attach & Detach Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void OnAttached()
        {
            // TODO Move the logic core else where! Or perhaps the components inside logic core.

            // Lets create logic core and filled data graph with edges and vertice
            var LogicCore = new NodeGraphLogicCore(); 

            // This property sets layout algorithm that will be used to calculate vertice positions
            // Different algorithms uses different values and some of them uses edge Weight property.
            LogicCore.DefaultLayoutAlgorithm = GraphX.LayoutAlgorithmTypeEnum.Custom;

            // Now we can set parameters for selected algorithm using AlgorithmFactory property. This property provides methods for
            // creating all available algorithms and algo parameters.
            //LogicCore.DefaultLayoutAlgorithmParams = LogicCore.AlgorithmFactory.CreateLayoutParameters(GraphX.LayoutAlgorithmTypeEnum.Tree);
            //((SimpleTreeLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).Direction = LayoutDirection.LeftToRight;
            //((SimpleTreeLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).VertexGap = 50;
            //((SimpleTreeLayoutParameters)LogicCore.DefaultLayoutAlgorithmParams).LayerGap = 100;

            // This property sets vertex overlap removal algorithm.
            // Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            LogicCore.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.None;
            //LogicCore.DefaultOverlapRemovalAlgorithmParams = LogicCore.AlgorithmFactory.CreateOverlapRemovalParameters(GraphX.OverlapRemovalAlgorithmTypeEnum.FSA);
            //((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            //((OverlapRemovalParameters)LogicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            // This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            // For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            // Bundling algorithm will try to tie different edges that follows same direction to a single channel making complex graphs more appealing.
            LogicCore.DefaultEdgeRoutingAlgorithm = GraphX.EdgeRoutingAlgorithmTypeEnum.SimpleER;

            // This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            // will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            // Area.RelayoutFinished and Area.GenerateGraphFinished.
            LogicCore.AsyncAlgorithmCompute = false;

            // Create the quick graph for the node
            LogicCore.Graph = new BidirectionalGraph<NodeVertex, NodeEdge>();
            graph = LogicCore.Graph;

            // Finally assign logic core to GraphArea object
            AssociatedObject.LogicCore = LogicCore;// as IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>;

            // Create the control factory
            AssociatedObject.ControlFactory = new NodeGraphControlFactory(AssociatedObject);

            //
            base.OnAttached();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDetaching()
        {
            // TODO
            base.OnDetaching();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVerticesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems) { AddNode(newItem as NodeVertex); }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in e.OldItems) { RemoveNode(oldItem as NodeVertex); }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEdgesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems) { AddEdge(newItem as NodeEdge); }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in e.OldItems) { RemoveEdge(oldItem as NodeEdge); }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #region Graph Operation Methods
        /// <summary>
        /// 
        /// </summary>
        public void RelayoutGraph()
        {
            AssociatedObject.RelayoutGraph();

            // TODO We might not need this
            /*foreach (var edge in AssociatedObject.EdgesList)
            {
                edge.Value.Visibility = Visibility.Visible;
            }*/
        }

        private void RecreateGraph(NodeGraphBehavior behavior)
        {
            // Disconnect all controls
            foreach (NodeVertex node in behavior.Vertices)
            {
                // Disconnect control
                VertexControl vertexControl;
                if (AssociatedObject.VertexList.TryGetValue(node, out vertexControl))
                    node.DisconnectControl(vertexControl);
            }

            // Loop through all the root nodes and add them
            behavior.graph.Clear();
            AssociatedObject.RemoveAllEdges();
            AssociatedObject.RemoveAllVertices();
            if (behavior.Vertices != null)
            {
                foreach (var item in behavior.Vertices)
                {
                    AddNode(item as NodeVertex);
                }
            }
            if (behavior.Edges != null)
            {
                foreach (var item in behavior.Edges)
                {
                    AddEdge(item as NodeEdge);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        protected void AddNode(NodeVertex node)
        {
            // Skip if it it already been added
            if (graph.ContainsVertex(node)) { return; }

            // Add the vertex to the logic graph 
            graph.AddVertex(node);

            // Create the vertex control
            var control = AssociatedObject.ControlFactory.CreateVertexControl(node);
            control.DataContext = node;
            control.Visibility = Visibility.Hidden; // make them invisible (there is no layout positions yet calculated)
            
            // Create data binding for input slots and output slots
            var binding = new Binding();
            binding.Path = new PropertyPath("InputSlots");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = node;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;            
            BindingOperations.SetBinding(control, NodeVertexControl.InputSlotsProperty, binding);

            binding = new Binding();
            binding.Path = new PropertyPath("OutputSlots");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = node;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(control, NodeVertexControl.OutputSlotsProperty, binding);
            
            // Add vertex and control to the graph area
            AssociatedObject.AddVertex(node, control);
            AssociatedObject.RelayoutGraph();

            // Connect control
            node.ConnectControl(control);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        protected void RemoveNode(NodeVertex node)
        {
            if (graph.ContainsVertex(node))
            {
                // TODO Need a better way to removing incoming edges
                IEnumerable<EdgeControl> controls = AssociatedObject.GetAllEdgeControls().Where(x => (x.Edge as NodeEdge).Target == node);
                foreach (var control in controls)
                {
                    NodeEdge edge = control.Edge as NodeEdge;
                    graph.RemoveEdge(edge);
                    AssociatedObject.RemoveEdge(edge);
                }

                // Disconnect control
                VertexControl vertexControl;
                if (AssociatedObject.VertexList.TryGetValue(node, out vertexControl))
                    node.DisconnectControl(vertexControl);

                // Then remove the vertex
                graph.RemoveVertex(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        protected void AddEdge(NodeEdge edge)
        {
            // Skip if it it already been added
            if (graph.ContainsEdge(edge)) { return; }

            // Add the vertex to the logic graph 
            graph.AddEdge(edge);            
            
            // Create the vertex control
            var control = AssociatedObject.ControlFactory.CreateEdgeControl(AssociatedObject.VertexList[edge.Source], AssociatedObject.VertexList[edge.Target], edge);
            control.DataContext = edge;
            control.Visibility = Visibility.Hidden; // make them invisible (there is no layout positions yet calculated)

            // Create data binding for input slots and output slots
            var binding = new Binding();
            binding.Path = new PropertyPath("SourceSlot");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = edge;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(control, NodeEdgeControl.SourceSlotProperty, binding);

            binding = new Binding();
            binding.Path = new PropertyPath("TargetSlot");
            binding.Mode = BindingMode.TwoWay;
            binding.Source = edge;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(control, NodeEdgeControl.TargetSlotProperty, binding);

            // Add vertex and control to the graph area
            AssociatedObject.AddEdge(edge, control);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edge"></param>
        protected void RemoveEdge(NodeEdge edge)
        {
            if (graph.ContainsEdge(edge))
            {
                graph.RemoveEdge(edge);
                AssociatedObject.RemoveEdge(edge);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        protected void RemoveEdge(NodeVertex source, NodeVertex target)
        {
            // TODO: Make an efficient remove
            var edgeToRemove = graph.Edges.Where(x => x.Source == source && x.Target == target).ToList();
            foreach (var edge in edgeToRemove)
            {
                graph.RemoveEdge(edge);
            }
        }
        #endregion
        
        #region Properties
        public IEnumerable Vertices { get { return (IEnumerable)GetValue(VerticesProperty); } set { SetValue(VerticesProperty, value); } }
        public IEnumerable Edges { get { return (IEnumerable)GetValue(EdgesProperty); } set { SetValue(EdgesProperty, value); } }
        public IList ConnectionWrappers { get { return connection_wrappers_; } }
        #endregion
    }
}
