using System.Runtime.Serialization;

namespace CartaCore.Integration.HyperThought.Data
{
    public enum ExportControl
    {
        [EnumMember(Value = "itar")]
        ITAR,
        [EnumMember(Value = "ear")]
        EAR
    }
}