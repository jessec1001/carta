using System.Collections.Generic;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents a template for an operation.
    /// </summary>
    public class OperationTemplate
    {
        /// <summary>
        /// The type of the operation. 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The subtype of the operation.
        /// For instance, the workflow identifier.
        /// </summary>
        public string Subtype { get; set; }

        /// <summary>
        /// The default values of the operation.
        /// </summary>
        public IDictionary<string, object> Default { get; set; }
    }
}