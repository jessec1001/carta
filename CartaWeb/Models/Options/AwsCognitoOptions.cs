namespace CartaWeb.Models.Options
{
    /// <summary>
    /// Options used to define Cognito parameters
    /// </summary>
    public class AwsCognitoOptions
    {
        /// <summary>
        /// Gets or sets the Cognito user pool ID
        /// </summary>
        /// <value>The Cognito user pool ID.</value>
        public string UserPoolId { get; set; }

        /// <summary>
        /// Gets or sets the previous Cognito user pool ID, if needed when a user pool is migrated.
        /// </summary>
        /// <value>The previous Cognito user pool ID.</value>
        public string PreviousUserPoolId { get; set; }
    }
}