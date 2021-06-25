using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using MorseCode.ITask;
using Amazon;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

using CartaCore.Workflow;
using CartaCore.Persistence;
using CartaCore.Serialization.Json;
using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;

namespace CartaWeb.Models.Migration
{
    /// <summary>
    /// Represents methods to migrate DynamoDB items for release dev_v0.2.3
    /// </summary>
    public class DynamoDbMigrator : INoSqlDbMigrator
    {
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        static DynamoDbMigrator()
        {
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.IgnoreNullValues = true;
            JsonOptions.Converters.Insert(0, new JsonDiscriminantConverter());
        }

        private DynamoDbContext _noSqlDbContext;

        private readonly IAmazonCognitoIdentityProvider _identityProvider;

        private string _backupARN;

        private string _userPoolId;


        /// <summary>
        /// Constructor that initializes private instance members, including the database context used to
        /// access DynamoDB, and the Cognito identity provider used to lookup user names for deployment of dev_v0.2.3.
        /// </summary>
        /// <param name="awsAccessKey">AWS application access key</param>
        /// <param name="awsSecretKey">AWS application secrect key</param>
        /// <param name="regionEndpoint">AWS region endpoint</param>
        /// <param name="tableName">The name of the table to migrate</param>
        /// <param name="cognitoUserPoolId">The Cognito user pool ID to use for looking up user names</param>
        public DynamoDbMigrator(
            string awsAccessKey,
            string awsSecretKey,
            RegionEndpoint regionEndpoint,
            string tableName,
            string cognitoUserPoolId
        )
        {
            _noSqlDbContext = new DynamoDbContext
                (
                    awsAccessKey,
                    awsSecretKey,
                    regionEndpoint,
                    tableName
                );
            _identityProvider = new AmazonCognitoIdentityProviderClient
                    (
                        awsAccessKey,
                        awsSecretKey,
                        regionEndpoint
                    );
            _userPoolId = cognitoUserPoolId;
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
            if (response.Users.Count == 0) throw new MigrationException($"User {userId} not found");
            else return response.Users[0].Username;
        }

        /// <summary>
        /// Backs up the database table.
        /// </summary>
        /// <returns>True if the backup was successful, otherwise false.</returns>
        public async ITask<bool> Backup()
        {
            Console.WriteLine("DynamoDbMigrator: Running backup...");
            CreateBackupRequest createBackupRequest = new CreateBackupRequest();
            createBackupRequest.BackupName = "DynamoDbMigrator_" + DateTime.Now.ToFileTimeUtc();
            createBackupRequest.TableName = _noSqlDbContext.TableName;
            CreateBackupResponse createBackupResponse =
                await _noSqlDbContext.Client.CreateBackupAsync(createBackupRequest);
            _backupARN = createBackupResponse.BackupDetails.BackupArn;
            // Check if backup completed before returning
            int i = 0;
            bool isBackedUp = false;
            while (!isBackedUp)
            {
                Console.WriteLine("DynamoDbMigrator: Waiting for backup...");
                Thread.Sleep(15000);
                if (i >= 20)
                {
                    throw new MigrationException($"Table backup request {createBackupRequest.BackupName} of table " +
                        $"{createBackupRequest.TableName} for ARN {_backupARN} has timed out");
                }
                i++;
                DescribeBackupRequest describeBackupRequest = new DescribeBackupRequest();
                describeBackupRequest.BackupArn = _backupARN;
                DescribeBackupResponse describeBackupResponse =
                    await _noSqlDbContext.Client.DescribeBackupAsync(describeBackupRequest);
                string backupStatus = describeBackupResponse.BackupDescription.BackupDetails.BackupStatus.ToString();
                Console.WriteLine($"DynamoDbMigrator: Current status of backup {_backupARN} is {backupStatus}");
                isBackedUp = backupStatus == "AVAILABLE";
            }
            return isBackedUp;
        }

        /// <summary>
        /// Performs the steps required to migrate the database table.
        /// Note that the implementation will be specific to the software release version and migration
        /// changes required for the specific version. 
        /// </summary>
        /// <returns>True if the migration steps were successful, otherwise false.</returns>
        public async ITask<bool> Migration()
        {
            Console.WriteLine("DynamoDbMigrator: Running migration...");

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("SK", ScanOperator.BeginsWith, "WORKFLOW#");
            Search search = _noSqlDbContext.DbTable.Scan(scanFilter);
            Console.WriteLine($"DynamoDbMigrator: Number of items to port is {search.Count}");
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
                        workflow,
                        new VersionInformation(0, null, new UserInformation(userId, userName))
                    );
                    jsonString = JsonSerializer.Serialize<WorkflowItem>(workflowItem, JsonOptions);
                    Console.WriteLine($"DynamoDbMigrator: Updating workflow item with PK={partitionKey} and " +
                        $"SK={sortKey} for userName={userName}");
                    await _noSqlDbContext.SaveDocumentStringAsync(partitionKey, sortKey, jsonString);               
                }
            } while (!search.IsDone);

            return true;
        }

        /// <summary>
        /// Restores the database table using the previously created backup.
        /// </summary>
        /// <returns>True if the restore was successful, otherwise false.</returns>
        public async ITask<bool> Rollback()
        {
            Console.WriteLine("DynamoDbMigrator: Running rollback...");

            // Restoring from a backup is not allowed on existing tables, so first delete the table
            DeleteTableRequest deleteRequest = new DeleteTableRequest();
            deleteRequest.TableName = _noSqlDbContext.TableName;
            DeleteTableResponse deleteResponse = await _noSqlDbContext.Client.DeleteTableAsync(deleteRequest);

            // Check that table is deleted before attempting to restore
            int i = 0;
            bool isTableDeleted = false;
            while (!isTableDeleted)
            {
                Console.WriteLine("DynamoDbMigrator: Waiting to delete existing table...");
                Thread.Sleep(15000);
                if (i >= 20)
                {
                    throw new MigrationException($"Table delete request has timed out");
                }
                i++;
                ListTablesResponse listTablesResponse = await _noSqlDbContext.Client.ListTablesAsync();
                isTableDeleted = !listTablesResponse.TableNames.Contains(_noSqlDbContext.TableName);
            }
            Console.WriteLine("DynamoDbMigrator: Existing table has been deleted");

            // Restore the table from backup
            RestoreTableFromBackupRequest restoreRequest = new RestoreTableFromBackupRequest();
            restoreRequest.BackupArn = _backupARN;
            restoreRequest.TargetTableName = _noSqlDbContext.TableName;
            RestoreTableFromBackupResponse response =
                await _noSqlDbContext.Client.RestoreTableFromBackupAsync(restoreRequest);

            // Ensure the table is available before returning
            i = 0;
            bool isTableAvailable = false;
            while (!isTableAvailable)
            {
                Console.WriteLine("DynamoDbMigrator: Waiting for restore to complete...");
                if (i >= 20)
                {
                    throw new MigrationException($"Table restore request of table {restoreRequest.TargetTableName} for ARN " +
                        $"{restoreRequest.BackupArn} has timed out");
                }
                i++;
                DescribeTableResponse tableStatus =
                    await _noSqlDbContext.Client.DescribeTableAsync(restoreRequest.TargetTableName);
                isTableAvailable = tableStatus.Table.TableStatus.ToString() == "ACTIVE";
                Thread.Sleep(15000);
            }
            Console.WriteLine("DynamoDbMigrator: Table is restored");
            return isTableAvailable;
        }

        /// <summary>
        /// Backs up the database table, after which migration steps are performed.
        /// If migration errors occur, the database table is restored. 
        /// </summary>
        /// <returns>True if the migration was successful, otherwise false.</returns>
        public async ITask<bool> Migrate()
        {
            bool isBackedUp = await Backup();
            try
            {
                if (isBackedUp)
                {
                    bool isMigrated = await Migration();
                    return isMigrated;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace.ToString());
                bool isRolledback = await Rollback();
                if (!isRolledback) Console.WriteLine("DynamoDbMigrator: ERROR! Attempt to rollback failed");
                throw;
            }
            return false;
        }

    }
}
