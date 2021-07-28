using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using MorseCode.ITask;
using Amazon;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;

using CartaCore.Workflow;
using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;
using CartaCore.Persistence;

namespace CartaWeb.Models.Migration
{
    /// <summary>
    /// Represents methods to migrate DynamoDB items for release dev v3.0
    /// </summary>
    public class DynamoDbMigratorWorkflowVersioning : DynamoDbMigrator
    {
        private readonly IAmazonCognitoIdentityProvider _identityProvider;

        private string _userPoolId;

        /// <summary>
        /// Constructor that initializes private instance members, including the database context used to
        /// access DynamoDB, and the Cognito identity provider used to lookup user names for deployment of devv0.2.3.
        /// </summary>
        /// <param name="awsAccessKey">AWS application access key</param>
        /// <param name="awsSecretKey">AWS application secrect key</param>
        /// <param name="regionEndpoint">AWS region endpoint</param>
        /// <param name="tableName">The name of the table to migrate</param>
        /// <param name="logger">The application log instance</param>
        public DynamoDbMigratorWorkflowVersioning(
            string awsAccessKey,
            string awsSecretKey,
            RegionEndpoint regionEndpoint,
            string tableName,
            ILogger logger
        ) : base(awsAccessKey, awsSecretKey, regionEndpoint, tableName, logger)
        {
            _identityProvider = new AmazonCognitoIdentityProviderClient
                (
                    awsAccessKey,
                    awsSecretKey,
                    regionEndpoint
                );
            if (tableName == "CartaDev") _userPoolId = "us-east-2_FqNrLUDlH";
            if (tableName == "Carta") _userPoolId = "us-east-2_MXndqWLaI";
        }

        /// <summary>
        /// Retrieves the user identifier from the given document item partition key.
        /// </summary>
        /// <param name="partitionKey">A document item partition key</param>
        /// <returns>The user identifier.</returns>
        private string GetUserId(String partitionKey)
        {
            return partitionKey.Remove(0, 5);
        }

        /// <summary>
        /// Retrieves the user name associated with the given user identifier.
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <returns>The user name.</returns>
        private async Task<string> GetUserName(string userId)
        {
            ListUsersRequest request = new ListUsersRequest();
            request.UserPoolId = _userPoolId;
            request.Filter = "sub=\"" + userId + "\"";
            ListUsersResponse response = await _identityProvider.ListUsersAsync(request);
            if (response.Users.Count == 0) throw new MigrationException(TableName, $"User {userId} not found");
            else return response.Users[0].Username;
        }

        /// <summary>
        /// Performs the steps required to migrate the database table.
        /// </summary>
        /// <returns>True if the migration steps were successful, otherwise false.</returns>
        public override async ITask<bool> Migrate()
        {
            Logger.LogInformation("Running migration...");
            
            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("SK", ScanOperator.BeginsWith, "WORKFLOW#");
            Search search = NoSqlDbContext.DbTable.Scan(scanFilter);
            Logger.LogInformation($"Number of items to port is {search.Count}");
            List <Document> documentList = new List<Document>();
            do
            {
                documentList = await search.GetNextSetAsync();     
                foreach (Document document in documentList)
                {
                    string partitionKey = document["PK"];
                    string sortKey = document["SK"];
                    document.Remove("PK");
                    document.Remove("SK");
                    string jsonString = document.ToJson();
                    Workflow workflow = JsonSerializer.Deserialize<Workflow>(jsonString, JsonOptions);
                    string userId = GetUserId(partitionKey);
                    string userName = await GetUserName(userId);
                    WorkflowItem workflowItem = new WorkflowItem
                    (
                        true,
                        userId,
                        workflow,
                        new VersionInformation(0, null, new UserInformation(userId, userName))
                    );
                    jsonString = JsonSerializer.Serialize<WorkflowItem>(workflowItem, JsonOptions);
                    Logger.LogInformation($"Updating workflow item with PK={partitionKey} and " +
                        $"SK={sortKey} for userName={userName}");
                    DbDocument dbDocument =
                        new DbDocument(partitionKey, sortKey, jsonString, DbOperationEnumeration.Save);
                    await NoSqlDbContext.WriteDocumentAsync(dbDocument);               
                }
            } while (!search.IsDone);
            
            return true;
        }
    }
}
