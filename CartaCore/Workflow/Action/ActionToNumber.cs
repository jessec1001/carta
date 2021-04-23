// using System;
// using System.Globalization;
// using System.Threading.Tasks;

// using CartaCore.Data;
// using CartaCore.Serialization;

// namespace CartaCore.Workflow.Action
// {
//     [DiscriminantDerived("to number")]
//     public class ActionToNumber : ActionBase
//     {
//         public override Task<object> ApplyToValue(object value)
//         {
//             if (value is string str && double.TryParse
//                 (
//                     str,
//                     NumberStyles.AllowDecimalPoint,
//                     CultureInfo.InvariantCulture,
//                     out double strNumber
//                 )
//             )
//                 return Task.FromResult((object)strNumber);
//             if (value is int intNumber)
//                 return Task.FromResult((object)Convert.ToDouble(intNumber));
//             if (value is double doubleNumber)
//                 return Task.FromResult((object)doubleNumber);
//             return Task.FromResult(value);
//         }
//     }
// }