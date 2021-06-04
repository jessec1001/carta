using System.Runtime.Serialization;

namespace CartaCore.Integration.Hyperthought
{
    /// <summary>
    /// Represents the type of a HyperThought file space.
    /// </summary>
    public enum HyperthoughtFileSpace
    {
        /// <summary>
        /// A user file space.
        /// </summary>
        [EnumMember(Value = "user")]
        User,
        /// <summary>
        /// A group file space.
        /// </summary>
        [EnumMember(Value = "group")]
        Group,
        /// <summary>
        /// A project file space.
        /// </summary>
        [EnumMember(Value = "project")]
        Project
    }
}