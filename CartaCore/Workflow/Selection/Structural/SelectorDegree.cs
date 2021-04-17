using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    [DiscriminantDerived("degree")]
    public class SelectorDegree : Selector
    {
        #region API Parameters
        public int? InDegree { get; set; } = null;
        public int? OutDegree { get; set; } = null;
        #endregion

        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            if (InDegree.HasValue && (
                vertex is not IInVertex inVertex ||
                inVertex.InEdges.Count() != InDegree
            )) return Task.FromResult(false);
            if (OutDegree.HasValue && (
                vertex is not IOutVertex outVertex ||
                outVertex.OutEdges.Count() != OutDegree
            )) return Task.FromResult(false);
            return Task.FromResult(true);
        }

        public override IAsyncEnumerable<IVertex> GetVertices()
        {
            if (
                InDegree == 0 && OutDegree == null &&
                Graph.TryProvide(out IRootedGraph rooted) &&
                Graph.TryProvide(out IDynamicGraph<IVertex> dynamic)
            ) return dynamic.GetVertices(rooted.GetRoots());
            else return base.GetVertices();
        }

        [DiscriminantAlias("roots")]
        public static SelectorDegree CreateRootsSelector() => new SelectorDegree
        {
            InDegree = 0,
            OutDegree = null
        };
    }
}