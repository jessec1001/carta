using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of all vertices.
    /// </summary>
    [DiscriminantDerived("all")]
    public class SelectorAll : SelectorBase { }
}