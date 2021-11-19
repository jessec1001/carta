using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

using CartaCore.Serialization.Json;
using CartaCore.Operations.Hyperthought.Data;

namespace CartaCore.Operations.Hyperthought
{

    /// <summary>
    /// The input for the <see cref="HyperthoughtPropertiesConvertOperation" /> operation.
    /// </summary>
    public struct InputHyperthoughtPropertiesConvertOperation
    {
        /// <summary>
        /// A JSON stream that is a list of <see cref="HyperthoughtProperty"/> objects.
        /// </summary>
        public Stream Stream;
        //public string Stream;
    }

    /// <summary>
    /// The output for the <see cref="HyperthoughtPropertiesConvertOperation" /> operation.
    /// </summary>
    public struct OutputHyperthoughtPropertiesConvertOperation
    {
        /// <summary>
        /// A list of <see cref="HyperthoughtProperty"/> objects.
        /// </summary>
        public List<HyperthoughtProperty> HyperthoughtProperties;
    }

    /// <summary>
    /// An operation that converts a JSON stream into a list of <see cref="HyperthoughtProperty"/> objects.
    /// </summary>
    public class HyperthoughtPropertiesConvertOperation
    {
        /// <summary>
        /// Perform the operation
        /// </summary>
        /// <param name="input">An <see cref="InputHyperthoughtPropertiesConvertOperation" /> instance.</param>
        /// <returns>An <see cref="OutputHyperthoughtPropertiesConvertOperation" /> instance.</returns>
        public async Task<OutputHyperthoughtPropertiesConvertOperation> Perform(
            InputHyperthoughtPropertiesConvertOperation input)
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            jsonOptions.Converters.Insert(0, new JsonPrimitiveConverter());
            List<HyperthoughtProperty> hyperthoughtProperties =
                await JsonSerializer.DeserializeAsync<List<HyperthoughtProperty>>(input.Stream, jsonOptions);
            return new OutputHyperthoughtPropertiesConvertOperation { HyperthoughtProperties = hyperthoughtProperties};
        }

    }

}