using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    [DiscriminantDerived("stringReplace")]
    public class ActorStringReplace : Actor
    {
        public string Pattern { get; set; }
        public string Replacement { get; set; }

        public override Task<object> TransformValue(object value)
        {
            if (value is string str)
            {
                str = Regex.Replace
                (
                    str, Pattern, Replacement,
                    RegexOptions.None,
                    TimeSpan.FromSeconds(1.0)
                );
                return Task.FromResult<object>(str);
            }
            else return Task.FromResult<object>(value);
        }
    }
}