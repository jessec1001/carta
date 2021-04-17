using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    [DiscriminantBase]
    public abstract class Selector : WrapperGraph,
        IEntireGraph
    {
        public IGraph Graph { get; set; }

        protected override IGraph WrappedGraph => Graph;

        public virtual Task<bool> ContainsVertex(IVertex vertex) => Task.FromResult(true);
        public virtual Task<bool> ContainsEdge(Edge edge) => Task.FromResult(true);
        public virtual Task<bool> ContainsProperty(Property property) => Task.FromResult(true);
        public virtual Task<bool> ContainsValue(object value) => Task.FromResult(true);

        public virtual IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
                return entire.GetVertices().WhereAwait(async vertex => await ContainsVertex(vertex));
            else throw new NotSupportedException();
        }
    }
}