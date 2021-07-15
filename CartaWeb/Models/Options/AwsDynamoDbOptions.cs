using System.Collections.Generic;

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
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the migration class for the table for new software releases
        /// </summary>
        /// <value>The migration class name</value>
        public string MigrationClass { get; set; }
    }
}