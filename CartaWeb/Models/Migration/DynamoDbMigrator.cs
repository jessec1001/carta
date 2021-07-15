using System;
using System.Threading;
using System.Text.Json;
using MorseCode.ITask;
using Amazon;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;

using CartaCore.Persistence;
using CartaCore.Serialization.Json;

namespace CartaWeb.Models.Migration
{
    /// <summary>
    /// A base class that codifies steps to migrate DynamoDB items. Steps to backup, rollback and perform migration will
    /// likely not change between software releases, and base implementations are provided. The specific steps
    /// required in the migration will vary between releases, and should be implemented by child classes.
    /// </summary>
    public abstract class DynamoDbMigrator : INoSqlDbMigrator
    {
        /// <summary>
        /// Options for serialization
        /// </summary>
        protected static JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        static DynamoDbMigrator()
        {
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.IgnoreNullValues = true;
            JsonOptions.Converters.Insert(0, new JsonDiscriminantConverter());
        }

        /// <summary>
        /// The NoSQL DB context for this migrator.
        /// </summary>
        protected DynamoDbContext NoSqlDbContext;

        /// <summary>
        /// The ARN identifying the table backup created during migration.
        /// </summary>
        protected string BackupARN;

        /// <summary>
        /// The name of the table being migrated.
        /// </summary>
        protected string TableName;

        /// <summary>
        /// The logger for this migrator.
        /// </summary>
        protected ILogger Logger;

        /// <summary>
        /// Constructor that initializes private instance members, including the database context used to
        /// access DynamoDB, and the Cognito identity provider used to lookup user names for deployment of devv0.2.3.
        /// </summary>
        /// <param name="awsAccessKey">AWS application access key</param>
        /// <param name="awsSecretKey">AWS application secrect key</param>
        /// <param name="regionEndpoint">AWS region endpoint</param>
        /// <param name="tableName">The name of the table to migrate</param>
        /// <param name="logger">The application log instance</param>
        public DynamoDbMigrator(
            string awsAccessKey,
            string awsSecretKey,
            RegionEndpoint regionEndpoint,
            string tableName,
            ILogger logger
        )
        {
            NoSqlDbContext = new DynamoDbContext
                (
                    awsAccessKey,
                    awsSecretKey,
                    regionEndpoint,
                    tableName
                );
            TableName = tableName;
            Logger = logger;
        }

        /// <summary>
        /// Backs up the database table.
        /// </summary>
        /// <returns>True if the backup was successful, otherwise false.</returns>
        public async ITask<bool> Backup()
        {
            Logger.LogInformation("Running backup...");
            CreateBackupRequest createBackupRequest = new CreateBackupRequest();
            createBackupRequest.BackupName = "DynamoDbMigrator" + DateTime.Now.ToFileTimeUtc();
            createBackupRequest.TableName = NoSqlDbContext.TableName;
            CreateBackupResponse createBackupResponse =
                await NoSqlDbContext.Client.CreateBackupAsync(createBackupRequest);
            BackupARN = createBackupResponse.BackupDetails.BackupArn;
            // Check if backup completed before returning
            int i = 0;
            bool isBackedUp = false;
            while (!isBackedUp)
            {
                Logger.LogInformation("Waiting for backup...");
                Thread.Sleep(15000);
                if (i >= 20)
                {
                    throw new MigrationException(TableName, $"Table backup request {createBackupRequest.BackupName} " +
                        $"for ARN {BackupARN} has timed out");
                }
                i++;
                DescribeBackupRequest describeBackupRequest = new DescribeBackupRequest();
                describeBackupRequest.BackupArn = BackupARN;
                DescribeBackupResponse describeBackupResponse =
                    await NoSqlDbContext.Client.DescribeBackupAsync(describeBackupRequest);
                string backupStatus = describeBackupResponse.BackupDescription.BackupDetails.BackupStatus.ToString();
                Logger.LogInformation($"Current status of backup {BackupARN} is {backupStatus}");
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
        public abstract ITask<bool> Migrate();

        /// <summary>
        /// Restores the database table using the previously created backup.
        /// </summary>
        /// <returns>True if the restore was successful, otherwise false.</returns>
        public async ITask<bool> Rollback()
        {
            Logger.LogInformation("Running rollback...");

            // Restoring from a backup is not allowed on existing tables, so first delete the table
            DeleteTableRequest deleteRequest = new DeleteTableRequest();
            deleteRequest.TableName = NoSqlDbContext.TableName;
            DeleteTableResponse deleteResponse = await NoSqlDbContext.Client.DeleteTableAsync(deleteRequest);

            // Check that table is deleted before attempting to restore
            int i = 0;
            bool isTableDeleted = false;
            while (!isTableDeleted)
            {
                Logger.LogInformation("Waiting to delete existing table...");
                Thread.Sleep(15000);
                if (i >= 20)
                {
                    throw new MigrationException(TableName, $"Table delete request has timed out");
                }
                i++;
                ListTablesResponse listTablesResponse = await NoSqlDbContext.Client.ListTablesAsync();
                isTableDeleted = !listTablesResponse.TableNames.Contains(NoSqlDbContext.TableName);
            }
            Logger.LogInformation("Existing table has been deleted");

            // Restore the table from backup
            RestoreTableFromBackupRequest restoreRequest = new RestoreTableFromBackupRequest();
            restoreRequest.BackupArn = BackupARN;
            restoreRequest.TargetTableName = NoSqlDbContext.TableName;
            RestoreTableFromBackupResponse response =
                await NoSqlDbContext.Client.RestoreTableFromBackupAsync(restoreRequest);

            // Ensure the table is available before returning
            i = 0;
            bool isTableAvailable = false;
            while (!isTableAvailable)
            {
                Logger.LogInformation("Waiting for restore to complete...");
                if (i >= 20)
                {
                    throw new MigrationException(TableName, $"Table restore request of table " +
                        $"{restoreRequest.TargetTableName} for ARN {restoreRequest.BackupArn} has timed out");
                }
                i++;
                DescribeTableResponse tableStatus =
                    await NoSqlDbContext.Client.DescribeTableAsync(restoreRequest.TargetTableName);
                isTableAvailable = tableStatus.Table.TableStatus.ToString() == "ACTIVE";
                Thread.Sleep(15000);
            }
            Logger.LogInformation("Table is restored");
            return isTableAvailable;
        }

        /// <summary>
        /// Backs up the database table, after which migration steps are performed.
        /// If migration errors occur, the database table is restored. 
        /// </summary>
        /// <returns>True if the migration was successful, otherwise false.</returns>
        public async ITask<bool> PerformMigration()
        {
            bool isBackedUp = await Backup();
            try
            {
                if (isBackedUp)
                {
                    bool isMigrated = await Migrate();
                    if (isMigrated) Logger.LogInformation("Migration completed successfully");
                    else Logger.LogError("ERROR! Migration failed");
                    return isMigrated;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace.ToString());
                bool isRolledback = await Rollback();
                if (!isRolledback) Logger.LogError("ERROR! Attempt to rollback failed");
                throw;
            }
            return false;
        }

    }
}
