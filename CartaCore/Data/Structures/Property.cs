using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaCore.Data
{
    /// <summary>
    /// Represents a property with any number of observations. This property may be attached to any element of a graph
    /// such as a vertex, an edge, or the graph itself.
    /// </summary>
    public class Property : Identifiable<Property>
    {
        /// <summary>
        /// Gets or sets the property observations.
        /// </summary>
        /// <value>The observations recorded for this property.</value>
        public IEnumerable<Observation> Observations { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with its specified identifier and a set of
        /// observations recorded for it.
        /// </summary>
        /// <param name="id">The identifier of this property.</param>
        /// <param name="observations">The observations recorded for this property.</param>
        public Property(Identity id, IEnumerable<Observation> observations)
            : base(id)
        {
            if (observations is null) throw new ArgumentNullException(nameof(observations));
            Observations = observations;
        }
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with its specified identifier.
        /// </summary>
        /// <param name="id">The identifier of this property.</param>
        public Property(Identity id)
            : this(id, Enumerable.Empty<Observation>()) { }
    }

    /// <summary>
    /// Represents a property that has a parent element with any number observations. This property should be attached
    /// to its parent element.
    /// </summary>
    /// <typeparam name="T">The type of parent element.</typeparam>
    public class Property<T> : Property where T : Element<T>
    {
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with a specified element to provide a parent
        /// identifier and a set of observations recorded for it.
        /// </summary>
        /// <param name="element">The parent element.</param>
        /// <param name="observations">The observations recorded for this property.</param>
        public Property(T element, IEnumerable<Observation> observations)
            : base(element.Identifier, observations) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with a specified element to provide a parent
        /// identifier.
        /// </summary>
        /// <param name="element">The parent element.</param>
        public Property(T element)
            : this(element, Enumerable.Empty<Observation>()) { }
        /// <summary>
        /// Initializes an isntance of the <see cref="Property"/> class with a specified element to provide a parent
        /// identifier and an auxilliary identifier, and a set of observations recorded for it.
        /// </summary>
        /// <param name="element">The parent element.</param>
        /// <param name="id">The auxilliary identifier.</param>
        /// <param name="observations">The observations recored for this property.</param>
        public Property(T element, Identity id, IEnumerable<Observation> observations)
            : base(Identity.Create(new CompoundIdentifier<T>(element, id)), observations) { }
        /// <summary>
        /// Initializes an instance of the <see cref="Property"/> class with a specified element to provide a parent
        /// identifier and an auxilliary identifier.
        /// </summary>
        /// <param name="element">The parent element.</param>
        /// <param name="id">The auxilliary identifier.</param>
        public Property(T element, Identity id)
            : this(element, id, Enumerable.Empty<Observation>()) { }
    }
}