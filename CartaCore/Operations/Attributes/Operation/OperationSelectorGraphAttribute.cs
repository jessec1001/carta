using System;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that indicates that a particular property is the graph that is the target of a selector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OperationSelectorGraphAttribute : Attribute { }
}