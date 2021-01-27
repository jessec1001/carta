using System.Text.Json.Serialization;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents the source of a data resource.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataSource
    {
        /// <summary>
        /// The internal, server-generated data source.
        /// </summary>
        Synthetic,
        /// <summary>
        /// The external data source from HyperThought.
        /// </summary>
        HyperThought
    }
}