using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Computes the means of any non-empty lists of numeric values. 
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("mean")]
    [DiscriminantSemantics(Name = "Sample Mean", Group = "Statistics")]
    public class ActorMean : Actor
    {
        /// <inheritdoc />
        public override Task<Property> TransformProperty(Property property)
        {
            try
            {
                // Get the numeric data.
                List<double> numbers = property.Values.Cast<double>().ToList();

                if (numbers.Count > 0)
                {
                    // Compute the mean.
                    double mean = numbers.Average();

                    // Assign the mean.
                    if (property.Subproperties is null)
                        property.Subproperties = Enumerable.Empty<Property>();
                    property.Subproperties = property.Subproperties.Append
                    (
                        // TODO: Make assigning property statistics easier.
                        new Property
                        (
                            Identity.Create("Mean"),
                            new object[] { mean }
                        )
                    );
                }
            }
            catch (InvalidCastException) { }
            return Task.FromResult(property);
        }
    }
}