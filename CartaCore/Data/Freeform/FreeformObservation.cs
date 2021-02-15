namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a particular observation of a freeform property.
    /// </summary>
    public class FreeformObservation
    {
        /// <summary>
        /// Gets or sets the observation value.
        /// </summary>
        /// <value>The observation value.</value>
        public object Value { get; set; }
        /// <summary>
        /// Gets or sets the observation type.
        /// </summary>
        /// <value>The observation type as a string representation.</value>
        public string Type { get; set; }
    }
}