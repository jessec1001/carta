using System;
namespace CartaWeb.Models.Migration
{
    /// <summary>
    /// Exception class used to denote that an exception occurred during database migration
    /// </summary>
    public class MigrationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MigrationException(string tableName, string message) : base(ModifyMessage(tableName, message)) { }

        private static string ModifyMessage(string tableName, string message)
        {
            return $"Migration error while migrating table {tableName}: {message}";
        }
    }
}

