namespace CartaCore.Data
{
    /// <summary>
    /// Represents an object that contains an identity that can be used to order and assert this object in relation to
    /// other objects of its kind.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// The identifier that uniquely represents this object.
        /// </summary>
        string Id { get; }
    }
}