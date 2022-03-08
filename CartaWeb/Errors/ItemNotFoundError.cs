namespace CartaWeb.Errors
{
    /// <summary>
    /// An error structure that represents when a requested item is not found.
    /// </summary>
    public struct ItemNotFoundError
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public string Error { get; }
        /// <summary>
        /// The identifier of the item that was not found.
        /// </summary>
        public string Id { get; }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNotFoundError"/> struct.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="id">The identifier of the item that was not found.</param>
        public ItemNotFoundError(string message, string id)
        {
            Error = message;
            Id = id;
        }
    }
}