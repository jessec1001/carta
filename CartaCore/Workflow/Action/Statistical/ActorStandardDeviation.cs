using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Computes the standard deviations of any non-empty lists of numeric values.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("deviation")]
    [DiscriminantSemantics(Name = "Sample Standard Deviation", Group = "Statistics")]
    public class ActorStandardDeviation : Actor
    {
        /// <summary>
        /// Whether to use [Bessel's Correction](https://en.wikipedia.org/wiki/Bessel%27s_correction) in the calculation
        /// of the standard deviation.
        /// </summary>
        [DataMember(Name = "besselCorrection")]
        [Display(Name = "Use Bessel's Correction")]
        [Required]
        public bool UseBesselCorrection { get; set; }

        /// <inheritdoc />
        public override Task<Property> TransformProperty(Property property)
        {
            try
            {
                // Get the numeric data.
                List<double> numbers;
                if (property.Value is IEnumerable<object> values)
                    numbers = values.Cast<double>().ToList();
                else
                    numbers = new List<double>();

                if (numbers.Count > 0)
                {
                    // Compute the mean.
                    double mean = numbers.Average();

                    // Compute the variance.
                    // Use Bessel's correction if specified.
                    double variance = 0;
                    if (UseBesselCorrection)
                    {
                        if (numbers.Count == 1) variance = double.MaxValue;
                        else variance = numbers.Select(number => Math.Pow(number - mean, 2.0)).Sum() / (numbers.Count - 1);
                    }
                    else
                    {
                        if (numbers.Count > 0) variance = 0;
                        else variance = numbers.Select(number => Math.Pow(number - mean, 2.0)).Sum() / numbers.Count;
                    }
                    double deviation = UseBesselCorrection && numbers.Count == 1
                        ? double.MaxValue
                        : Math.Sqrt(variance);

                    // Assign the variance.
                    if (property.Subproperties is null)
                        property.Subproperties = Enumerable.Empty<Property>();
                    property.Subproperties = property.Subproperties.Append
                    (
                        // TODO: Make assigning property statistics easier.
                        new Property
                        (
                            Identity.Create("Standard Deviation"),
                            new object[] { deviation }
                        )
                    );
                }
            }
            catch (InvalidCastException) { }
            return Task.FromResult(property);
        }
    }
}