using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Represents an action that can act on or transform vertices, edges, or a graph itself.
    /// </summary>
    [DiscriminantBase]
    public abstract class ActionBase
    {
        /// <summary>
        /// Applies the action to a specified vertex.
        /// </summary>
        /// <param name="vertex">The vertex to apply to action to.</param>
        /// <returns>The vertex after the action has been applied.</returns>
        public virtual Task<IVertex> ApplyToVertex(IGraph graph, IVertex vertex) => Task.FromResult(vertex);
        public virtual Task<Property> ApplyToProperty(Property property) => Task.FromResult(property);
        public virtual Task<object> ApplyToValue(object value) => Task.FromResult(value);
    }
}