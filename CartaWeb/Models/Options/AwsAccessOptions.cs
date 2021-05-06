namespace CartaWeb.Models.Options
{
    /// <summary>
    /// Options used to define an AWS IAM user's access to AWS.
    /// </summary>
    public class AwsAccessOptions
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
    }
}