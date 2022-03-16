namespace CartaWeb.Models.Options
{
    /// <summary>
    /// Options used to define properties set by CDK deployment
    /// </summary>
    public class AwsCdkOptions
    {
        /// <summary>
        /// Gets or sets the access key.
        /// </summary>
        /// <value>The public AWS access key.</value>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the secret key.
        /// </summary>
        /// <value>The secret AWS access key.</value>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the region endpoint
        /// </summary>
        /// <value>The region endpoint, e.g. us-east-2.</value>
        public string RegionEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the DynamoDB table name
        /// </summary>
        /// <value>The name of the DynamoDB table.</value>
        public string DynamoDBTable { get; set; }

        /// <summary>
        /// Gets or sets the secrets DynamoDB table name
        /// </summary>
        /// <value>The name of the secrets DynamoDB table.</value>
        public string SecretsDynamoDBTable { get; set; }

        /// <summary>
        /// Gets or sets the Cognito user pool ID
        /// </summary>
        /// <value>The Cognito user pool ID.</value>
        public string UserPoolId { get; set; }

        /// <summary>
        /// Gets or sets the Cognito user pool client ID
        /// </summary>
        /// <value>The Cognito user pool client ID.</value>
        public string UserPoolClientId { get; set; }
    }
}
