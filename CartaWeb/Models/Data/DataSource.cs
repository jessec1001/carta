using System.Text.Json.Serialization;

using CartaWeb.Models.Meta;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Represents the source of a data resource.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [ApiType(typeof(string))]
    public enum DataSource
    {
        /// <summary>
        /// The internal, server-generated data source.
        /// </summary>
        Synthetic,
        /// <summary>
        /// The internal, user-generated data source.
        /// </summary>
        User,
        /// <summary>
        /// The external data source from HyperThought.
        /// </summary>
        HyperThought
    }
}