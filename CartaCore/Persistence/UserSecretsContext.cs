using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Represents a database context for accessing user secrets stored in a special DynamoDB table
    /// </summary>
    public class UserSecretsContext : IUserSecretsContext
    {
        /// <summary>
        /// Partition key column name 
        /// </summary>
        const string _pk = "UserId";

        /// <summary>
        /// Sort key colum name 
        /// </summary>
        const string _sk = "Key";

        /// <summary>
        /// Value column name 
        /// </summary>
        const string _val = "Value";

        /// <summary>
        /// Gets or sets the DynamoDb client through which calls are made
        /// </summary>
        /// <value>The DynamoDb client</value>
        public AmazonDynamoDBClient Client { get; set; }

        /// <summary>
        /// Gets or sets the DynamoDb table name
        /// </summary>
        /// <value>The DynamoDb table</value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the DynamoDb table
        /// </summary>
        /// <value>The DynamoDb table</value>
        public Table DbTable { get; set; }

        /// <summary>
        /// Constructor for local instance
        /// </summary>
        public UserSecretsContext(AmazonDynamoDBClient client, string tableName)
        {
            // Set the DynamoDB client
            Client = client;

            // Load the DynamoDB table
            TableName = tableName;
            DbTable = Table.LoadTable(Client, TableName);
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public UserSecretsContext
        (
            string awsAccessKey,
            string awsSecretKey,
            Amazon.RegionEndpoint regionEndpoint,
            string tableName
        )
        {
            // Initialize DynamoDb client
            if (awsAccessKey is null)
                Client = new AmazonDynamoDBClient(regionEndpoint);
            else
                Client = new AmazonDynamoDBClient(
                    awsAccessKey,
                    awsSecretKey,
                    regionEndpoint);

            // Load the DynamoDB table
            TableName = tableName;
            DbTable = Table.LoadTable(Client, TableName);
        }


        /// <inheritdoc />
        public async Task<string> GetUserSecretAsync(string userId, string secretKey)
        {
            Task<Document> task = DbTable.GetItemAsync(userId, secretKey);
            Document item = await task;
            return item[_val];
        }

        /// <inheritdoc />
        public async Task PutUserSecretAsync(string userId, string secretKey, string secretValue)
        {
            Document item = new Document();
            item.Add(_pk, userId);
            item.Add(_sk, secretKey);
            item.Add(_val, secretValue);
            await DbTable.PutItemAsync(item);
        }
    }
}
