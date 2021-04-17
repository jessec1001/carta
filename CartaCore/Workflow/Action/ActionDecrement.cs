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
    public class ActionDecrement : ActionBase
    {
        /// <summary>
        /// Gets or sets the decrement amount.
        /// </summary>
        /// <value>The amount to decrement integer values by.</value>
        public double Amount { get; set; } = 1.0;

        public override Task<object> ApplyToValue(object value)
        {
            if (value is double number)
                return Task.FromResult((object)(number - Amount));
            else
                return Task.FromResult(value);
        }
    }
}