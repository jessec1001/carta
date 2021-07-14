using System.Runtime.Serialization;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Converts any non-string values to strings.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("toString")]
    [DiscriminantSemantics(Name = "Convert to String", Group = "Conversion")]
    public class ActorToString : Actor
    {
        /// <inheritdoc />
        public override Task<object> TransformValue(object value)
        {
            return Task.FromResult<object>(value.ToString());
        }
    }
}