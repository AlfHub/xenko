﻿using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public abstract class NodePresenterBase : IInitializingNodePresenter
    {
        private readonly INodePresenterFactoryInternal factory;
        private readonly List<INodePresenter> children = new List<INodePresenter>();
        private HashSet<INodePresenter> dependencies;

        protected NodePresenterBase([NotNull] INodePresenterFactoryInternal factory, [CanBeNull] IPropertyProviderViewModel propertyProvider, [CanBeNull] INodePresenter parent)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.factory = factory;
            Parent = parent;
            PropertyProvider = propertyProvider;
        }

        public virtual void Dispose()
        {
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    dependency.ValueChanged -= DependencyChanged;
                }
            }
        }

        public INodePresenter this[string childName] => children.First(x => string.Equals(x.Name, childName, StringComparison.Ordinal));

        public INodePresenter Root => Parent?.Root ?? Parent ?? this;

        public INodePresenter Parent { get; private set; }

        public IReadOnlyList<INodePresenter> Children => children;

        public string DisplayName { get; set; }
        public string Name { get; protected set; }

        public List<INodePresenterCommand> Commands { get; } = new List<INodePresenterCommand>();
        public abstract Type Type { get; }
        public abstract bool IsEnumerable { get; }

        public bool IsVisible { get; set; } = true;
        public bool IsReadOnly { get; set; }
        public int? Order { get; set; }

        public abstract Index Index { get; }
        public abstract ITypeDescriptor Descriptor { get; }
        public abstract object Value { get; }
        public string CombineKey { get; set; }
        public PropertyContainerClass AttachedProperties { get; } = new PropertyContainerClass();

        public event EventHandler<ValueChangingEventArgs> ValueChanging;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected abstract IObjectNode ParentingNode { get; }

        public abstract void UpdateValue(object newValue);

        public abstract void AddItem(object value);

        public abstract void AddItem(object value, Index index);

        public abstract void RemoveItem(object value, Index index);

        public abstract NodeAccessor GetNodeAccessor();

        public IPropertyProviderViewModel PropertyProvider { get; }

        public INodePresenterFactory Factory => factory;

        public override string ToString()
        {
            return $"[{GetType().Name}] {Name} (Count = {Children.Count}";
        }

        public void ChangeParent(INodePresenter newParent)
        {
            if (newParent == null) throw new ArgumentNullException(nameof(newParent));

            var parent = (NodePresenterBase)Parent;
            parent?.children.Remove(this);

            parent = (NodePresenterBase)newParent;
            parent.children.Add(this);

            Parent = newParent;
        }

        public void Rename(string newName, bool overwriteCombineKey = true)
        {
            Name = newName;
            if (overwriteCombineKey)
            {
                CombineKey = newName;
            }
        }

        public void AddDependency([NotNull] INodePresenter node, bool refreshOnNestedNodeChanges)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            dependencies = dependencies ?? new HashSet<INodePresenter>();        
            if (dependencies.Add(node))
            {
                node.ValueChanged += DependencyChanged;
            }
        }

        protected void Refresh()
        {
            // Remove existing children and attached properties
            foreach (var child in children.DepthFirst(x => x.Children))
            {
                child.Dispose();
            }
            children.Clear();
            AttachedProperties.Clear();

            // And recompute them from the current value.
            factory.CreateChildren(this, ParentingNode, PropertyProvider);
        }

        protected void AttachCommands()
        {
            foreach (var command in factory.AvailableCommands)
            {
                if (command.CanAttach(this))
                    Commands.Add(command);
            }
        }

        protected void RaiseValueChanging(object newValue)
        {
            ValueChanging?.Invoke(this, new ValueChangingEventArgs(newValue));
        }

        protected void RaiseValueChanged(object oldValue)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(oldValue));
        }

        private void DependencyChanged(object sender, ValueChangedEventArgs e)
        {
            RaiseValueChanging(Value);
            Refresh();
            RaiseValueChanged(Value);
        }

        void IInitializingNodePresenter.AddChild([NotNull] IInitializingNodePresenter child)
        {
            children.Add(child);
        }

        void IInitializingNodePresenter.FinalizeInitialization()
        {
        }
    }
}
