using System;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that indicates that an operation should be hidden by default lookup behaviors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OperationHiddenAttribute : Attribute { }
}