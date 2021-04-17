using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    [DiscriminantDerived("exclude")]
    public class SelectExclude : Selector
    {
        #region API Parameters
        public List<string> Ids { get; set; }
        #endregion

        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            return Task.FromResult(!Ids.Any(id => Identity.Create(id).Equals(vertex.Identifier)));
        }
    }
}