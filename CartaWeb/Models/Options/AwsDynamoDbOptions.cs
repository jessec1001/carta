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

        /// <summary>
        /// Gets or sets a flag for performing database migration.
        /// </summary>
        /// <value>true if dabase migration should be performed, else false.</value>
        public bool Migrate { get; set; }
    }
}