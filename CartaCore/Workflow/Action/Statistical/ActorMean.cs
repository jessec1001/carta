using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    [DiscriminantDerived("mean")]
    public class ActorMean : Actor
    {
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