using System.Collections.Generic;
using System.Linq;

using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    [DiscriminantDerived("exclude vertex")]
    public class SelectExclude : SelectorBase
    {
        public List<string> Ids { get; set; }

        public override bool ContainsVertex(IVertex vertex)
        {
            return !Ids.Any
            (
                id => vertex.Identifier.Equals(Identity.Create(id))
            );
        }
    }
}