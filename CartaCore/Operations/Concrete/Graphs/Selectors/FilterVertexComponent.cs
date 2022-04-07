using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Graphs.Components;
using MorseCode.ITask;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// A helper component for that allows for easily filtering out vertices from a graph using a function.
    /// </summary>
    public class FilterVertexComponent :
        IComponent,
        IEnumerableComponent<Vertex, Edge>,
        IDynamicLocalComponent<Vertex, Edge>,
        IDynamicInComponent<Vertex, Edge>,
        IDynamicOutComponent<Vertex, Edge>
    {
        /// <inheritdoc />
        public ComponentStack Components { get; set; }

        /// <summary>
        /// The filter that is used to filter out vertices that should not be included in a graph.
        /// This function returns true if the vertex should be included, false otherwise.
        /// </summary>
        public Func<Vertex, Task<bool>> Filter { get; private init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterVertexComponent"/> class.
        /// </summary>
        /// <param name="filter">The filter to apply to vertices.</param>
        public FilterVertexComponent(Func<Vertex, Task<bool>> filter) => Filter = filter;

        /// <summary>
        /// Attaches this component to a graph.
        /// </summary>
        /// <param name="graph">The graph.</param>
        public void Attach(Graph graph)
        {
            // Branch the graph component stack.
            graph.Components = graph.Components.Branch();

            // We remove the rooted component because the filter makes us uncertain whether the roots remain untouched.
            graph.Components.Remove<IRootedComponent>();

            // We add all other components conditionally on whether an existing component is present.
            if (graph.Components.TryFind<IEnumerableComponent<Vertex, Edge>>(out _))
                graph.Components.Append<IEnumerableComponent<Vertex, Edge>>(this);
            if (graph.Components.TryFind<IDynamicLocalComponent<Vertex, Edge>>(out _))
                graph.Components.Append<IDynamicLocalComponent<Vertex, Edge>>(this);
            if (graph.Components.TryFind<IDynamicInComponent<Vertex, Edge>>(out _))
                graph.Components.Append<IDynamicInComponent<Vertex, Edge>>(this);
            if (graph.Components.TryFind<IDynamicOutComponent<Vertex, Edge>>(out _))
                graph.Components.Append<IDynamicOutComponent<Vertex, Edge>>(this);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetVertices()
        {
            if (Components.TryFind<IEnumerableComponent<Vertex, Edge>>(out var enumerable))
            {
                await foreach (Vertex vertex in enumerable.GetVertices())
                {
                    if (await Filter(vertex))
                        yield return vertex;
                }
            }
            else throw new InvalidOperationException("The graph does not have an enumerable component.");
        }

        /// <inheritdoc />
        public async ITask<Vertex> GetVertex(string id)
        {
            if (Components.TryFind<IDynamicLocalComponent<Vertex, Edge>>(out var dynamicLocal))
            {
                Vertex vertex = await dynamicLocal.GetVertex(id);
                if (vertex is not null && await Filter(vertex)) return vertex;
                else return null;
            }
            else throw new InvalidOperationException("The graph does not have a dynamic local component.");
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetParentVertices(string id)
        {
            if (Components.TryFind<IDynamicInComponent<Vertex, Edge>>(out var dynamicIn))
            {
                await foreach (Vertex vertex in dynamicIn.GetParentVertices(id))
                {
                    if (await Filter(vertex))
                        yield return vertex;
                }
            }
            else throw new InvalidOperationException("The graph does not have a dynamic in component.");
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<Vertex> GetChildVertices(string id)
        {
            if (Components.TryFind<IDynamicOutComponent<Vertex, Edge>>(out var dynamicOut))
            {
                await foreach (Vertex vertex in dynamicOut.GetChildVertices(id))
                {
                    if (await Filter(vertex))
                        yield return vertex;
                }
            }
            else throw new InvalidOperationException("The graph does not have a dynamic out component.");
        }
    }
}