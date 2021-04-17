using CartaCore.Serialization;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Represents a selection of all vertices.
    /// </summary>
    [DiscriminantDerived("all")]
    public class SelectorAll : Selector { }
}