namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An interface for an attribute that will be given an operation context before being applied.
    /// </summary>
    public interface IOperationJobAttribute
    {
        /// <summary>
        /// An operation context that is provided to the attribute.
        /// </summary>
        OperationJob Job { set; }
    }
}