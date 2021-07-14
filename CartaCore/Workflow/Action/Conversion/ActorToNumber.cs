using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Converts any non-number values that can be parsed as a number to numbers.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("toNumber")]
    [DiscriminantSemantics(Name = "Convert to Number", Group = "Conversion")]
    public class ActorToNumber : Actor
    {
        /// <inheritdoc />
        public override Task<object> TransformValue(object value)
        {
            if (value is string str && double.TryParse
                (
                    str,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out double strNumber
                )
            )
                return Task.FromResult<object>(strNumber);
            if (value is int intNumber)
                return Task.FromResult<object>(Convert.ToDouble(intNumber));
            if (value is double doubleNumber)
                return Task.FromResult<object>(doubleNumber);
            return Task.FromResult<object>(value);
        }
    }
}