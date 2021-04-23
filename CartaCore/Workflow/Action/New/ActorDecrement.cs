using System;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Represents an action that decrements integer observations by some amount.
    /// </summary>
    [DiscriminantDerived("decrement")]
    public class ActionDecrement : Actor
    {
        /// <summary>
        /// Gets or sets the decrement amount.
        /// </summary>
        /// <value>The amount to decrement integer values by.</value>
        public double Amount { get; set; } = 1.0;

        public override object TransformValue(object value)
        {
            if (value is double number)
                return number - Amount;
            else
                return value;
        }
    }
}