using System.Collections.Generic;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents the base structure of a property on an element.
    /// </summary>
    public interface IProperty : IIdentifiable
    {
        /// <summary>
        /// The value of the property.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// The subproperties of this property.
        /// </summary>
        ISet<IProperty> Properties { get; }
    }
}