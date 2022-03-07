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
        IEnumerableComponent<IVertex, IEdge>,
        IDynamicLocalComponent<IVertex, IEdge>,
        IDynamicInComponent<IVertex, IEdge>,
        IDynamicOutComponent<IVertex, IEdge>
    {
        /// <inheritdoc />
        public ComponentStack Components { get; set; }

        /// <summary>
        /// The filter that is used to filter out vertices that should not be included in a graph.
        /// This function returns true if the vertex should be included, false otherwise.
        /// </summary>
        public Func<IVertex, Task<bool>> Filter { get; private init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterVertexComponent"/> class.
        /// </summary>
        /// <param name="filter">The filter to apply to vertices.</param>
        public FilterVertexComponent(Func<IVertex, Task<bool>> filter) => Filter = filter;
    
        /// <summary>
        /// Attaches this component to a graph.
        /// </summary>
        /// <param name="graph">The graph.</param>
        public void Attach(Graph graph)
        {
            // We remove the rooted component because the filter makes us uncertain whether the roots remain untouched.
            graph.Components.RemoveAll<IRootedComponent>();

            // We add all other components conditionally on whether an existing component is present.
            if (graph.Components.TryFind<IEnumerableComponent<IVertex, IEdge>>(out _))
                graph.Components.AddTop<IEnumerableComponent<IVertex, IEdge>>(this);
            if (graph.Components.TryFind<IDynamicLocalComponent<IVertex, IEdge>>(out _))
                graph.Components.AddTop<IDynamicLocalComponent<IVertex, IEdge>>(this);
            if (graph.Components.TryFind<IDynamicInComponent<IVertex, IEdge>>(out _))
                graph.Components.AddTop<IDynamicInComponent<IVertex, IEdge>>(this);
            if (graph.Components.TryFind<IDynamicOutComponent<IVertex, IEdge>>(out _))
                graph.Components.AddTop<IDynamicOutComponent<IVertex, IEdge>>(this);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Components.TryFind<IEnumerableComponent<IVertex, IEdge>>(out var enumerable))
            {
                await foreach (IVertex vertex in enumerable.GetVertices())
                {
                    if (await Filter(vertex))
                        yield return vertex;
                }
            }
            else throw new InvalidOperationException("The graph does not have an enumerable component.");
        }

        /// <inheritdoc />
        public async ITask<IVertex> GetVertex(string id)
        {
            if (Components.TryFind<IDynamicLocalComponent<IVertex, IEdge>>(out var dynamicLocal))
            {
                IVertex vertex = await dynamicLocal.GetVertex(id);
                if (vertex is not null && await Filter(vertex)) return vertex;
                else return null;
            }
            else throw new InvalidOperationException("The graph does not have a dynamic local component.");
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IVertex> GetParentVertices(string id)
        {
            if (Components.TryFind<IDynamicInComponent<IVertex, IEdge>>(out var dynamicIn))
            {
                await foreach (IVertex vertex in dynamicIn.GetParentVertices(id))
                {
                    if (await Filter(vertex))
                        yield return vertex;
                }
            }
            else throw new InvalidOperationException("The graph does not have a dynamic in component.");
        }
        /// <inheritdoc />
        public async IAsyncEnumerable<IVertex> GetChildVertices(string id)
        {
            if (Components.TryFind<IDynamicOutComponent<IVertex, IEdge>>(out var dynamicOut))
            {
                await foreach (IVertex vertex in dynamicOut.GetChildVertices(id))
                {
                    if (await Filter(vertex))
                        yield return vertex;
                }
            }
            else throw new InvalidOperationException("The graph does not have a dynamic out component.");
        }
    }
}