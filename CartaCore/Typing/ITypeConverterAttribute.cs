using System.Collections.Generic;
using CartaCore.Typing.Conversion;

namespace CartaCore.Typing.Attributes
{
    /// <summary>
    /// A base interface for attributes that modify a type converter stack.
    /// </summary>
    public interface ITypeConverterAttribute
    {
        /// <summary>
        /// Applies the attribute to the given type converter stack.
        /// </summary>
        /// <param name="converters">The current converter stack.</param>
        void ApplyConverter(IList<TypeConverter> converters);
    }
}