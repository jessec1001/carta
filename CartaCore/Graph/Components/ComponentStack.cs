using System.Collections.Generic;

namespace CartaCore.Graph.Components
{
    /// <summary>
    /// A stack of components that can be retrieved via type.
    /// </summary>
    public class ComponentStack
    {
        /// <summary>
        /// Used to store the components in the stack.
        /// </summary>
        private readonly LinkedList<object> _components = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentStack"/> class.
        /// </summary>
        public ComponentStack() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentStack"/> class from a specified collection.
        /// </summary>
        /// <param name="collection">The collection of components.</param>
        public ComponentStack(IEnumerable<object> collection) => _components = new LinkedList<object>(collection);

        /// <summary>
        /// Adds a component to the top of the stack.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <typeparam name="T">The type of component.</typeparam>
        public void AddTop<T>(T component)
        {
            _components.AddFirst(component);
        }
        /// <summary>
        /// Adds a component to the bottom of the stack.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <typeparam name="T">The type of component.</typeparam>
        public void AddBottom<T>(T component)
        {
            _components.AddLast(component);
        }

        /// <summary>
        /// Removes the top component from the stack.
        /// </summary>
        /// <returns>Whether the removal was successful.</returns>
        public bool RemoveTop()
        {
            if (_components.Count == 0)
                return false;
            _components.RemoveFirst();
            return true;
        }
        /// <summary>
        /// Removes the bottom component from the stack.
        /// </summary>
        /// <returns>Whether the removal was successful.</returns>
        public bool RemoveBottom()
        {
            if (_components.Count == 0)
                return false;
            _components.RemoveLast();
            return true;
        }
        /// <summary>
        /// Removes the topmost component of the specified type from the stack.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>Whether the removal was successful.</returns>
        public bool Remove<T>()
        {
            LinkedListNode<object> componentNode = _components.First;
            while (componentNode is not null)
            {
                if (componentNode.Value is T)
                {
                    _components.Remove(componentNode);
                    return true;
                }
                componentNode = componentNode.Next;
            }
            return false;
        }
        /// <summary>
        /// Removes all of the components of the specified type from the stack.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The number of components removed.</returns>
        public int RemoveAll<T>()
        {
            int count = 0;
            LinkedListNode<object> componentNode = _components.First;
            while (componentNode is not null)
            {
                if (componentNode.Value is T)
                {
                    _components.Remove(componentNode);
                    count++;
                }
                componentNode = componentNode.Next;
            }
            return count;
        }

        /// <summary>
        /// Finds the topmost component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The topmost component of the specified type if found; otherwise, the default value.</returns>
        public T Find<T>()
        {
            foreach (var component in _components)
                if (component is T found) return found;
            return default;
        }
        /// <summary>
        /// FInds all components of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>The enumeration of components of the specified type.</returns>
        public IEnumerable<T> FindAll<T>()
        {
            foreach (var component in _components)
                if (component is T found) yield return found;
        }
        /// <summary>
        /// Tries to find the topmost component of the specified type.
        /// </summary>
        /// <param name="component">
        /// The first component of the specified type if found; otherwise, the default value.
        /// </param>
        /// <typeparam name="T">The type of component.</typeparam>
        /// <returns>Whether a component of the correct type was found.</returns>
        public bool TryFind<T>(out T component)
        {
            foreach (var c in _components)
                if (c is T found)
                {
                    component = found;
                    return true;
                }
            component = default;
            return false;
        }
    
        /// <summary>
        /// Clears the component stack.
        /// </summary>
        public void Clear()
        {
            _components.Clear();
        }
    }
}