using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using CartaCore.Serialization.Json;
using CartaCore.Operations.Hyperthought.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Hyperthought
{

    /// <summary>
    /// The input for the <see cref="ParseHyperthoughtPropertiesJsonOperation" /> operation.
    /// </summary>
    public struct ParseHyperthoughtPropertiesOperationIn
    {
        /// <summary>
        /// A JSON stream that is a list of <see cref="HyperthoughtProperty"/> objects.
        /// </summary>
        public Stream Stream { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="ParseHyperthoughtPropertiesJsonOperation" /> operation.
    /// </summary>
    public struct ParseHyperthoughtPropertiesOperationOut
    {
        /// <summary>
        /// A list of <see cref="HyperthoughtProperty"/> objects.
        /// </summary>
        public List<HyperthoughtProperty> HyperthoughtProperties { get; set; }
    }

    /// <summary>
    /// Converts a JSON stream into a list of <see cref="HyperthoughtProperty"/> objects.
    /// </summary>
    [OperationName(Display = "Parse HyperThought Properties JSON", Type = "hyperthoughtParsePropertiesJson")]
    [OperationTag(OperationTags.Hyperthought)]
    [OperationTag(OperationTags.Parsing)]
    public class ParseHyperthoughtPropertiesJsonOperation : TypedOperation
    <
        ParseHyperthoughtPropertiesOperationIn,
        ParseHyperthoughtPropertiesOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<ParseHyperthoughtPropertiesOperationOut> Perform(ParseHyperthoughtPropertiesOperationIn input)
        {
            // Create the JSON serializer options.
            JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
            jsonOptions.Converters.Insert(0, new JsonPrimitiveConverter());

            // Read the JSON stream.
            List<HyperthoughtProperty> hyperthoughtProperties =
                await JsonSerializer.DeserializeAsync<List<HyperthoughtProperty>>(input.Stream, jsonOptions);

            return new ParseHyperthoughtPropertiesOperationOut { HyperthoughtProperties = hyperthoughtProperties };
        }

    }

}