using System.Collections.Generic;

namespace CartaCore.Graph
{
    /// <summary>
    /// Represents the base structure of a property on an element.
    /// </summary>
    public interface IProperty
    {
        /// <summary>
        /// The value of the property.
        /// </summary>
        object Value { get; }
        
        /// <summary>
        /// The subproperties of this property.
        /// </summary>
        IDictionary<string, IProperty> Properties { get; }
    }
}