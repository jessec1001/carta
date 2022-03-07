using System;

namespace CartaCore.Operations.Attributes
{
    public class FieldCorrelationAttribute : Attribute
    {
        public string Correlated { get; init; }

        public FieldCorrelationAttribute(string correlated) => Correlated = correlated;
        public FieldCorrelationAttribute() : this(null) { }
    }
}