using MorseCode.ITask;
namespace CartaWeb.Models.Migration
{
    /// <summary>
    /// Represents methods to migrate items in a NoSQL database table as required for a new software release.
    /// </summary>
    public interface INoSqlDbMigrator
    {
        /// <summary>
        /// Backs up the database table, or items in the table that will be migrated.
        /// If migration errors occur, the database table will be restored from the backed up resources.
        /// </summary>
        /// <returns>True if the backup was successful, otherwise false.</returns>
        ITask<bool> Backup();

        /// <summary>
        /// Performs the steps required to migrate the database table.
        /// </summary>
        /// <returns>True if the migration steps were successful, otherwise false.</returns>
        ITask<bool> Migrate();

        /// <summary>
        /// Restores the database table using resources created during backup. 
        /// </summary>
        /// <returns>True if the rollback was successful, otherwise false.</returns>
        ITask<bool> Rollback();

        /// <summary>
        /// Backs up the database table, after which migration steps are performed.
        /// If migration errors occur, the database table is restored. 
        /// </summary>
        /// <returns>True if the migration was successful, otherwise false.</returns>
        ITask<bool> PerformMigration();
    }
}

