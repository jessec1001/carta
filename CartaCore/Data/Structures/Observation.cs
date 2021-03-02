namespace CartaCore.Data
{
    /// <summary>
    /// Represents a particular observation of a property.
    /// </summary>
    public class Observation : Identifiable<Observation>
    {
        /// <summary>
        /// Gets or sets the observation value.
        /// </summary>
        /// <value>The observation value.</value>
        public object Value { get; init; }
        /// <summary>
        /// Gets or sets the observation type.
        /// </summary>
        /// <value>The observation type as a string representation.</value>
        public string Type { get; init; }

        /// <summary>
        /// Initializes an instance of the <see cref="Observation"/> class with the specified identifier.
        /// </summary>
        /// <param name="id"></param>
        public Observation(Identity id)
            : base(id) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Observation"/> class.
        /// </summary>
        public Observation()
            : this(null) { }
    }
}