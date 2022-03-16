using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Authentication
{
    /// <summary>
    /// Represents the information necessary to authenticate a HyperThought user.
    /// </summary>
    public struct HyperthoughtAuthentication
    {
        /// <summary>
        /// The key for the authentication.
        /// </summary>
        public const string Key = "hyperthought";

        /// <summary>
        /// Your HyperThought™ API key is availabe in your [user profile](https://www.hyperthought.io/api/common/my_account/)
        /// on the HyperThought™ website. **Important:** this is sensitive information that could be used to forge
        /// your identity - do not misplace this information of allow it to be leaked.
        /// </summary>
        [FieldRequired]
        [FieldName("API Key")]
        public string ApiKey { get; set; }
    }
}