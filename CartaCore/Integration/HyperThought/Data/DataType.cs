using System.Runtime.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public enum DataType
    {
        [EnumMember(Value = "string")]
        String,
        [EnumMember(Value = "link")]
        Link
    }
}