using System;
using System.Globalization;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    [DiscriminantDerived("toNumber")]
    public class ActorToNumber : Actor
    {
        public override object TransformValue(object value)
        {
            if (value is string str && double.TryParse
                (
                    str,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out double strNumber
                )
            )
                return (object)strNumber;
            if (value is int intNumber)
                return Convert.ToDouble(intNumber);
            if (value is double doubleNumber)
                return doubleNumber;
            return value;
        }
    }
}