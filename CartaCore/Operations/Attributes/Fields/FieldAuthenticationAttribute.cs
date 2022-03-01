using System;

namespace CartaCore.Operations.Attributes
{
    public class FieldAuthenticationAttribute : Attribute
    {
        /// <summary>
        /// The type of structure that should be used for authentication.
        /// For instance, this may be <see cref="string" /> for an authentication token or a more complex structure
        /// containing a username or password for a database connection.
        /// </summary>
        public Type AuthenticationType { get; init; }
        /// <summary>
        /// The key that should be used when receiving authentication information from a client.
        /// </summary>
        public string Key { get; init; }
    
        public FieldAuthenticationAttribute(string key, Type authenticationType)
        {
            AuthenticationType = authenticationType;
            Key = key;
        }
    }
}