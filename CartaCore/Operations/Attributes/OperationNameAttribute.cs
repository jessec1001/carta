namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that assigns type and display names to an operation. 
    /// </summary>
    public class OperationNameAttribute : OperationDescribingAttribute
    {
        /// <summary>
        /// The type name of the operation. This will be unique across all operations and can be used as an
        /// identifier/discriminant for the particular operation code-type. 
        /// </summary>
        public string Type { get; init; }
        /// <summary>
        /// The display name of the operation. This need not be unique across all operations. However, it should provide
        /// a self-explanatory name to the operation that allows a user to intuitively guess the functionality of the
        /// operation.
        /// </summary>
        public string Display { get; init; }

        /// <inheritdoc />
        public override OperationDescription Modify(OperationDescription description)
        {
            description.Type = Type;
            description.Display = Display;
            return description;
        }
    }
}