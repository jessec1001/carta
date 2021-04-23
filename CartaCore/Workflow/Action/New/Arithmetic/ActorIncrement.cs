using System;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Represents an action that increments integer observations by some amount.
    /// </summary>
    [DiscriminantDerived("increment")]
    public class ActorIncrement : Actor
    {
        /// <summary>
        /// Gets or sets the increment amount.
        /// </summary>
        /// <value>The amount to increment integer values by.</value>
        public double Amount { get; set; } = 1.0;

        public override object TransformValue(object value)
        {
            if (value is double number)
                return number + Amount;
            else
                return value;
        }
    }
}