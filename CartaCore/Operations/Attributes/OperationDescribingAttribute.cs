using System;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that describes an operation and should thus modify its description.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class OperationDescribingAttribute : Attribute
    {
        /// <summary>
        /// Modifies an operation description by the attribute.
        /// </summary>
        /// <param name="description">The operation description to modify.</param>
        /// <returns>The modified operation description.</returns>
        public abstract OperationDescription Modify(OperationDescription description);
    }
}