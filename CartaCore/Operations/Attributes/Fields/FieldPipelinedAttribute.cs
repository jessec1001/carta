using System;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that marks an output field as a pipeline of values from this input field.
    /// Should only be specified on an input field of a typed operation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FieldPipelinedAttribute : Attribute
    {
        /// <summary>
        /// The name of the field that this field pipelines to.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldPipelinedAttribute"/> class with a field name.
        /// </summary>
        /// <param name="name">The name of the corresponding pipelined attribute.</param>
        public FieldPipelinedAttribute(string name) => Name = name;
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldPipelinedAttribute"/> class with the same field name.
        /// </summary>
        public FieldPipelinedAttribute() : this(null) { }
    }
}