namespace CartaCore.Operations
{
    /// <summary>
    /// A description of an operation including its unique type identity, naming, description, and tagging information.
    /// </summary>
    public struct OperationDescription
    {
        /// <summary>
        /// The type name of an operation. This will be unique across all operations and can be used as an
        /// identifier/discriminant for a particular operation code-type. 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The subtype name of an operation. This applies to operations of type "workflow" and similar. This will be
        /// unique per templated operation of a specific <see cref="Type" />.
        /// </summary>
        public string Subtype { get; set; }

        // TODO: Can we move this selector information somewhere else?
        /// <summary>
        /// The name of this operation when used as a selector for a graph. If the operation is not a selector, this
        /// value will be null.
        /// </summary>
        public string Selector { get; set; }
        /// <summary>
        /// The display name of an operation. This need not be unique across all operations. However, it should provide
        /// a self-explanatory name to an operation that allows a user to intuitively guess the functionality of an
        /// operation.
        /// </summary>
        public string Display { get; set; }

        /// <summary>
        /// A more detailed description of an operation functionality. This should be where in-depth details are given
        /// to allow the user to understand more about how an operation is performed. This field should be assumed to
        /// support Markdown syntax when interpreted by a client application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A collection of descriptive categories that apply to an operation. These should be designed to provide
        /// additional context clues to the nature of an operation. 
        /// </summary>
        public string[] Tags { get; set; }
    }
}