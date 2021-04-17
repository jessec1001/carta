using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    [DiscriminantDerived("include")]
    public class SelectorInclude : Selector
    {
        #region API Parameters
        public List<string> Ids { get; set; }
        #endregion

        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            return Task.FromResult(Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)));
        }

        public override IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
                return dynamic.GetVertices(Ids.Select(id => Identity.Create(id)));
            else return base.GetVertices();
        }
    }
}