using System;

namespace CartaCore.Serialization
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DiscriminantAliasAttribute : Attribute
    {
        public string AliasName { get; protected init; }

        public DiscriminantAliasAttribute(string aliasName)
        {
            AliasName = aliasName;
        }
    }
}