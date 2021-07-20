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
        /// Gets or sets table migration steps
        /// </summary>
        /// <value>A sorted dictionary of migration steps, with key set to the step name, and the
        /// value set to the migration class name</value>
        public SortedDictionary<string, string> MigrationSteps { get; set; }
    }
}
