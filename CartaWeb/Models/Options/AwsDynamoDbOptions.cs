namespace CartaWeb.Models.Options
{
    /// <summary>
    /// Options used to define access to a DynamoDB store.
    /// </summary>
    public class AwsDynamoDbOptions
    {
        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>The name of the DynamoDB table.</value>
        public string Table { get; set; }
    }
}