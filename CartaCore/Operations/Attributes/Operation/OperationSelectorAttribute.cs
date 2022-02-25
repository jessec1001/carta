using System;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that indicates that an operation is a selector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OperationSelectorAttribute : Attribute, IOperationDescribingAttribute
    {
        /// <summary>
        /// The name of the operation when used as a selector for a graph.
        /// </summary>
        public string Selector { get; init; }

        /// <summary>
        /// Assigns a selector name to the operation.
        /// </summary>
        /// <param name="selector">The selector name to assign.</param>
        public OperationSelectorAttribute(string selector) => Selector = selector;

        /// <inheritdoc />
        public OperationDescription Modify(OperationDescription description)
        {
            description.Selector = Selector;
            return description;
        }
    }
}