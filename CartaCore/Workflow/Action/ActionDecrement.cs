using CartaCore.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Represents an action that decrements integer observations by some amount.
    /// </summary>
    [DiscriminantDerived("decrement")]
    public class ActionDecrement : ActionBase
    {
        /// <summary>
        /// Gets or sets the decrement amount.
        /// </summary>
        /// <value>The amount to decrement integer values by.</value>
        public int Amount { get; set; } = 1;

        /// <inheritdoc />
        public override IVertex ApplyToVertex(IVertex vertex)
        {
            if (vertex is null) return vertex;
            foreach (Property property in vertex.Properties)
            {
                foreach (Observation observation in property.Observations)
                {
                    if (observation.Value is int value)
                        observation.Value = (value - Amount);
                }
            }
            return vertex;
        }
    }
}