namespace CartaCore.Persistence
{
    /// <summary>
    /// An enumeration of the possible database operations.
    /// </summary>
    public enum DbOperationType
    {
        /// <summary>
        /// Create operation.
        /// </summary>
        Create,
        /// <summary>
        /// Read operation.
        /// </summary>
        Read,
        /// <summary>
        /// Update operation.
        /// </summary>
        Update,
        /// <summary>
        /// Save operation.
        /// </summary>
        Save,
        /// <summary>
        /// Delete operation.
        /// </summary>
        Delete
    }
}
