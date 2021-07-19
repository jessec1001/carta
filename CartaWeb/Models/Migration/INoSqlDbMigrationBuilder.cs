using MorseCode.ITask;
namespace CartaWeb.Models.Migration
{
    /// <summary>
    /// Represents methods to perform migrations for one or more NoSQL database tables 
    /// </summary>
    public interface INoSqlDbMigrationBuilder
    {
        /// <summary>
        /// Performs a set of database table migrations
        /// </summary>
        ITask PerformMigrations();
    }
}
