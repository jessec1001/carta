using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MorseCode.ITask;
using Amazon;

namespace CartaWeb.Models.Migration
{
    /// <summary>
    /// Runs migration for each table and configuration class configured for migration.
    /// </summary>
    public class DynamoDbMigrationBuilder : INoSqlDbMigrationBuilder
    {
        private SortedDictionary<string, string> _migrations;
        private string _awsAccessKey;
        private string _awsSecretKey;
        private RegionEndpoint _regionEndpoint;
        private string _tableName;
        private ILogger _logger;

        /// <summary>
        /// Constructor that initializes private instance members, including a dictionary of migration classes,
        /// information needed to initialize the database context used to access DynamoDB, and logging.
        /// </summary>
        /// <param name="migrations">A dictionary of migrations, with key=tableName, and value=migrationClass</param>
        /// <param name="awsAccessKey">AWS application access key</param>
        /// <param name="awsSecretKey">AWS application secrect key</param>
        /// <param name="regionEndpoint">AWS region endpoint</param>
        /// <param name="logger">The application log instance</param>
        public DynamoDbMigrationBuilder(
            SortedDictionary<string, string> migrations,
            string awsAccessKey,
            string awsSecretKey,
            RegionEndpoint regionEndpoint,
            string tableName,
            ILogger logger)
        {
            _migrations = migrations;
            _awsAccessKey = awsAccessKey;
            _awsSecretKey = awsSecretKey;
            _regionEndpoint = regionEndpoint;
            _tableName = tableName;
            _logger = logger;
        }

        /// <summary>
        /// Runs a set of table migrations
        /// </summary>
        public async ITask PerformMigrations()
        {
            _logger.LogInformation("Running migrations....");

            foreach (KeyValuePair<string, string> pair in _migrations)
            {
                string stepName = pair.Key;
                string className = pair.Value;
                _logger.LogInformation($"Running migration for step {stepName} using classname {className}...");

                try
                {

                    DynamoDbMigrator dynamoDbMigrator = (DynamoDbMigrator)Activator.CreateInstance
                        (
                            Type.GetType(className),
                            new Object[]
                            {   _awsAccessKey,
                                _awsSecretKey,
                                _regionEndpoint,
                                _tableName,
                                _logger
                            }
                        );
                    await dynamoDbMigrator.PerformMigration();
                }
                catch (Exception e)
                {
                    _logger.LogError(e.StackTrace.ToString());
                    _logger.LogError(e.Message);
                    throw new MigrationException(_tableName, $"Migration performed by {className} failed unexpectedly");
                }
            }
        }
    }
}
