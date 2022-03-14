using System.Threading.Tasks;
using System;

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
        /// <param name="timeSpan">Time span for retaining the secret</param>
        Task PutUserSecretAsync(string userId, string secretKey, string secretValue, TimeSpan timeSpan);

    }
}

