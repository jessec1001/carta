namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An interface for an attribute that describes an operation and should thus modify its description.
    /// </summary>
    public interface IOperationDescribingAttribute
    {
        /// <summary>
        /// Modifies an operation description by the attribute.
        /// </summary>
        /// <param name="description">The operation description to modify.</param>
        /// <returns>The modified operation description.</returns>
        OperationDescription Modify(OperationDescription description);
    }
}