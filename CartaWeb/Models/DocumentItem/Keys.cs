﻿namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Provide methods to return database table item keys with required prefixes
    /// </summary>
    public class Keys
    {
        /// <summary>
        /// Returns a properly formatted user key.
        /// </summary>
        /// <param name="userId">A user identifier.</param>
        /// <returns>
        /// The key for the persisted item for the given user.
        /// </returns>
        public static string GetUserKey(string userId)
        {
            return "USER#" + userId;
        }

        /// <summary>
        /// Returns a properly formatted workspace key.
        /// </summary>
        /// <param name="workspaceId">A workspace identifier.</param>
        /// <returns>
        /// The key for the persisted item for the given workspace.
        /// </returns>
        public static string GetWorkspaceKey(string workspaceId)
        {
            return "WORKSPACE#" + workspaceId;
        }

        /// <summary>
        /// Returns a properly formatted data set key.
        /// </summary>
        /// <param name="datasetId">A dataest identifier.</param>
        /// <returns>
        /// The key for the persisted item for the given dataset.
        /// </returns>
        public static string GetDatasetKey(string datasetId)
        {
            return "DATASET#" + datasetId;
        }

        /// <summary>
        /// Returns a properly formatted workflow key.
        /// </summary>
        /// <param name="workflowId">A unique workflow ID.</param>
        /// <returns>
        /// The workflow key
        /// </returns>
        public static string GetWorkflowKey(string workflowId)
        {
            return "WORKFLOW#" + workflowId;
        }

        /// <summary>
        /// Returns a properly formatted workflow access key.
        /// </summary>
        /// <param name="workflowId">A unique workflow ID.</param>
        /// <returns>
        /// The workflow key
        /// </returns>
        public static string GetWorkflowAccessKey(string workflowId)
        {
            return "WORKFLOWACCESS#" + workflowId;
        }

        /// <summary>
        /// Returns a properly formatted version key.
        /// </summary>
        /// <param name="versionNumber">Version number.</param>
        /// <returns>
        /// The version key
        /// </returns>
        public static string GetVersionKey(int versionNumber)
        {
            return "VERSION#" + versionNumber;
        }

        /// <summary>
        /// Returns a properly formatted version key prefix
        /// </summary>
        /// <returns>
        /// The version key prefix
        /// </returns>
        public static string GetVersionKeyPrefix()
        {
            return "VERSION#";
        }
    }
}
