using System.Threading.Tasks;

namespace CartaCore.Persistence
{
    /// <summary>
    /// Represents a database context for accessing user secrets
    /// </summary>
    public interface IUserSecretsContext
    {
        /// <summary>
        /// Retrieves the user secret
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="secretKey">The secret key</param>
        /// <returns>The secret value </returns>
        Task<string> GetUserSecretAsync(string userId, string secretKey);

        /// <summary>
        /// Puts the user secret
        /// </summary>
        /// <param name="userId">The user identifier</param>
        /// <param name="secretKey">The secret key</param>
        /// <param name="secretValue">The secret value</param>
        Task PutUserSecretAsync(string userId, string secretKey, string secretValue);

    }
}

