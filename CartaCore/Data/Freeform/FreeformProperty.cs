using System.Collections.Generic;

namespace CartaCore.Data.Freeform
{
    /// <summary>
    /// Represents a property with any number of observations of a freeform graph vertex.
    /// </summary>
    public class FreeformProperty : FreeformObjectBase<FreeformProperty>
    {
        /// <summary>
        /// Gets or sets the list of observations for this property.
        /// </summary>
        /// <value>The property observations.</value>
        public IEnumerable<FreeformObservation> Observations { get; set; }

        /// <summary>
        /// Initializes an instance of the <see cref="FreeformProperty"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The property identifier.</param>
        public FreeformProperty(FreeformIdentity id)
            : base(id) { }
    }
}