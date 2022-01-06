namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that indicates that an operation is a visualization.
    /// </summary>
    public class OperationVisualizerAttribute : OperationDescribingAttribute
    {
        /// <summary>
        /// The name of the visualization. This is used for easier compatibility with the visualization library.
        /// </summary>
        public string Visualization { get; init; }

        /// <summary>
        /// Assigns a visualization name to the operation.
        /// </summary>
        /// <param name="visualization">The visualization name to assign.</param>
        public OperationVisualizerAttribute(string visualization) => Visualization = visualization;

        /// <inheritdoc />
        public override OperationDescription Modify(OperationDescription description)
        {
            description.Visualization = Visualization;
            return description;
        }
    }
}