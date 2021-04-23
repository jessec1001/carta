// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// using CartaCore.Data;
// using CartaCore.Serialization;

// namespace CartaCore.Workflow.Action
// {
//     [DiscriminantDerived("deviation")]
//     public class ActionStandardDeviation : ActionBase
//     {
//         public override Task<Property> ApplyToProperty(Property property)
//         {
//             try
//             {
//                 // Get the numeric data.
//                 List<double> numbers = property.Values.Cast<double>().ToList();

//                 if (numbers.Count > 0)
//                 {
//                     // Compute the mean.
//                     double mean = numbers.Average();

//                     // Compute the variance.
//                     double variance = numbers.Select(number => Math.Pow(number - mean, 2.0)).Sum() / numbers.Count;
//                     double deviation = Math.Sqrt(variance);

//                     // Assign the variance.
//                     if (property.Subproperties is null)
//                         property.Subproperties = Enumerable.Empty<Property>();
//                     property.Subproperties = property.Subproperties.Append
//                     (
//                         new Property
//                         (
//                             Identity.Create("Standard Deviation"),
//                             new object[] { deviation }
//                         )
//                     );
//                 }
//             }
//             catch (InvalidCastException) { }
//             return Task.FromResult(property);
//         }
//     }
// }