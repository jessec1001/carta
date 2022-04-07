using System;
using System.Collections.Generic;

namespace CartaCore.Graphs.Components
{
    /// <summary>
    /// A stack of components that can be retrieved via type.
    /// </summary>
    public class ComponentStack
    {
        /// <summary>
        /// Used to store the components in the local stack.
        /// </summary>
        private readonly LinkedList<(Type, IComponent)> _components;
        /// <summary>
        /// A reference the parent stack (if it exists).
        /// </summary>
        private readonly ComponentStack _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentStack"/> class by cloning another stack.
        /// </summary>
        /// <param name="components">The components.</param>
        /// <param name="parent">The parent stack.</param>
        private ComponentStack(LinkedList<(Type, IComponent)> components, ComponentStack parent)
        {
            _components = new();
            foreach ((Type, IComponent) component in components)
                _components.AddLast(component);
            _parent = parent;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentStack"/> class.
        /// </summary>
        public ComponentStack()
        {
            _components = new();
            _parent = new(new(), null);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentStack"/> class from a specified collection.
        /// </summary>
        /// <param name="collection">The collection of components.</param>
        public ComponentStack(IEnumerable<(Type, IComponent)> collection)
        {
            _components = new LinkedList<(Type, IComponent)>(collection);
            _parent = new(new(), null);
        }

        /// <summary>
        /// Appends a component to the local stack.
        /// This component will not be visible when retrieving components locally.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The stack.</returns>
        public ComponentStack Append<T>(T component) where T : IComponent
        {
            _components.AddFirst((typeof(T), component));
            component.Components = _parent;
            return this;
        }
        /// <summary>
        /// Removes a component of the specified type from the local stack.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The stack.</returns>
        public ComponentStack Remove<T>() where T : IComponent
        {
            // We remove components by appending a null component to the stack.
            _components.AddFirst((typeof(T), null));
            return this;
        }
        /// <summary>
        /// Clears the local stack.
        /// </summary>
        /// <returns>The stack.</returns>
        public ComponentStack Clear()
        {
            _components.Clear();
            return this;
        }

        /// <summary>
        /// Branches the local component stack from a new local component stack.
        /// This should be used to modify the local component stack without affecting the parent stack.
        /// </summary>
        /// <returns>The stack.</returns>
        public ComponentStack Branch()
            => new(new(), this);

        /// <summary>
        /// Finds the topmost component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The topmost component of the specified type if found; otherwise, the default value.</returns>
        public T Find<T>() where T : IComponent
        {
            foreach ((_, IComponent component) in _components)
                if (component is T found) return found;
            return _parent is null ? default : _parent.Find<T>();
        }
        /// <summary>
        /// Finds all components of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The enumeration of components of the specified type.</returns>
        public IEnumerable<T> FindAll<T>() where T : IComponent
        {
            foreach ((_, IComponent component) in _components)
                if (component is T found) yield return found;
            if (_parent is null) yield break;
            foreach (T component in _parent.FindAll<T>())
                yield return component;
        }
        /// <summary>
        /// Tries to find the topmost component of the specified type.
        /// </summary>
        /// <param name="component">
        /// The first component of the specified type if found; otherwise, the default value.
        /// </param>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>Whether a component of the correct type was found.</returns>
        public bool TryFind<T>(out T component) where T : IComponent
        {
            foreach ((Type type, IComponent component) pair in _components)
            {
                if (pair.component is T found)
                {
                    component = found;
                    return true;
                }
            }
            if (_parent is not null)
                return _parent.TryFind<T>(out component);

            component = default;
            return false;
        }
    }
}