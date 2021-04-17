using System;

namespace CartaCore.Serialization
{
    /// <summary>
    /// An attribute that can be placed on base types such as an abstract class or interface that specifies that derived
    /// classes can be differentiated from each other using a string value, the discriminant, that is given a certain
    /// name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DiscriminantBaseAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the type discriminant.
        /// </summary>
        /// <value>
        /// The name that should be used, when serializing or enumerating an object, that the discriminant should be
        /// given.
        /// </value>
        public string DiscriminantName { get; protected init; }

        /// <summary>
        /// Initializes an instance of the <see cref="DiscriminantBaseAttribute"/> class with a specified name for the
        /// discriminant.
        /// </summary>
        /// <param name="discriminantName">The name of the type discriminant.</param>
        public DiscriminantBaseAttribute(string discriminantName = "type")
        {
            DiscriminantName = discriminantName;
        }
    }
}