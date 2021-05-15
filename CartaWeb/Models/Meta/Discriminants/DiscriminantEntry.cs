using System.Text.Json.Serialization;

namespace CartaWeb.Models.Meta
{
    /// <summary>
    /// Represents an entry in a collection of discriminant names and groupings.
    /// </summary>
    public class DiscriminantEntry
    {
        /// <summary>
        /// Gets or sets whether the discriminant entry is hidden.
        /// </summary>
        /// <value><c>true</c> if the discriminant should be displayed by interfaces; otherwise <c>false</c>.</value>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the discriminant entry name.
        /// </summary>
        /// <value>The name of the discriminant.</value>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the discriminant entry group.
        /// </summary>
        /// <value>The logical grouping of the discriminant. Can be <c>null</c>.</value>
        [JsonPropertyName("group")]
        public string Group { get; set; }
    }
}