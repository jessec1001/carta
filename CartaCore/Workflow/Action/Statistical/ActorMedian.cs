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
    /// Computes the medians of any non-empty lists of numeric values.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("median")]
    [DiscriminantSemantics(Name = "Sample Median", Group = "Statistics")]
    public class ActorMedian : Actor
    {
        /// <inheritdoc />
        public override Task<Property> TransformProperty(Property property)
        {
            try
            {
                // Get the numeric data.
                List<double> numbers = property.Values.Cast<double>().ToList();
                numbers.Sort();

                if (numbers.Count > 0)
                {
                    // Compute the median.
                    double median = 0.0;
                    if (numbers.Count % 2 == 0)
                    {
                        int index1 = (numbers.Count - 1) / 2;
                        int index2 = index1 + 1;
                        median = (numbers[index1] + numbers[index2]) / 2.0;
                    }
                    else
                    {
                        int index = (numbers.Count - 1) / 2;
                        median = numbers[index];
                    }

                    // Assign the median.
                    if (property.Subproperties is null)
                        property.Subproperties = Enumerable.Empty<Property>();
                    property.Subproperties = property.Subproperties.Append
                    (
                        // TODO: Make assigning property statistics easier.
                        new Property
                        (
                            Identity.Create("Median"),
                            new object[] { median }
                        )
                    );
                }
            }
            catch (InvalidCastException) { }

            return Task.FromResult(property);
        }
    }
}