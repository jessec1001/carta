using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Increments numeric observations by some specified amount.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("increment")]
    [DiscriminantSemantics(Name = "Increment", Group = "Arithmetic")]
    public class ActorIncrement : Actor
    {
        /// <summary>
        /// The amount to increment values by.
        /// </summary>
        [DataMember(Name = "amount")]
        [Display(Name = "Amount")]
        [Required]
        public double Amount { get; set; } = 1.0;

        /// <inheritdoc />
        public override Task<object> TransformValue(object value)
        {
            // TODO: Make it simpler to check if a value is a numeric value or not.
            if (value is double numberDouble)
                return Task.FromResult<object>((double)(numberDouble + Amount));
            if (value is float numberFloat)
                return Task.FromResult<object>((float)(numberFloat + Amount));
            if (value is long numberLong)
                return Task.FromResult<object>((long)(numberLong + Amount));
            if (value is int numberInt)
                return Task.FromResult<object>((int)(numberInt + Amount));

            return Task.FromResult<object>(value);
        }
    }
}