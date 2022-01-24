using System.Collections.Generic;
using System.Text.Json.Serialization;
using CartaCore.Operations;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store information about an operation instance.
    /// </summary>
    public class OperationItem : Item
    {
        /// <summary>
        /// The specified type of predefined operation that this operation is an instance of.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The specified subtype of operation that this operation is an instance of.
        /// This is set to a workflow identifier when the operation is an instance of a workflow.
        /// </summary>
        public string Subtype { get; set; }

        /// <summary>
        /// The name of the operation. By default, this matches the display name of the operation type.
        /// </summary>
        public string Name { get; set; }

        // TODO: Prevent overposting to non-valid fields in defaults.
        /// <summary>
        /// The default input values specified for the operation.
        /// </summary>
        public Dictionary<string, object> Default { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationItem"/> class.
        /// </summary>
        public OperationItem() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationItem"/> class with specified instance of an operation.
        /// </summary>
        /// <param name="operation">The operation instance.</param>
        public OperationItem(Operation operation)
            : base(operation.Identifier)
        {
            // Get a description of the operation.
            OperationDescription description = OperationDescription.FromInstance(operation);

            // Set the values from the description.
            Type = description.Type;
            Subtype = description.Subtype;

            Default = operation.DefaultValues;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationItem"/> class with specified owning user from an
        /// instance of an operation.
        /// </summary>
        /// <param name="operationId">The operation instance.</param>
        public OperationItem(string operationId)
            : base(null, operationId) { }

        /// <inheritdoc />
        [JsonIgnore]
        public override string PartitionKeyPrefix => "OPERATION#ALL";
        /// <inheritdoc />
        [JsonIgnore]
        public override string SortKeyPrefix => "OPERATION#";
    }
}