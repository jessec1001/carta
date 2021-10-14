using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the functionality of the Workspace module of the HyperThought API.
    /// </summary>
    public class HyperthoughtWorkspacesApi
    {
        private HyperthoughtApi Api { get; init; }

        /// <summary>
        /// Gets the workspace API URI at the HyperThought instance.
        /// </summary>
        protected Uri GetApiUri() => new(Api.GetBaseUri(), "workspace/");
        /// <summary>
        /// Gets a workspace URI for a specified workspace ID at the HyperThought instance.
        /// </summary>
        protected Uri GetWorkspaceUri(Guid workspaceId) => new(GetApiUri(), $"{workspaceId}/");

        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtWorkspacesApi"/> class with the specified base API.
        /// </summary>
        /// <param name="api">The base HyperThought API.</param>
        public HyperthoughtWorkspacesApi(HyperthoughtApi api)
        {
            Api = api;
        }

        /// <summary>
        /// Obtains the list of HyperThought workspaces that the HyperThought user has access to.
        /// </summary>
        /// <returns>The list of workspaces obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkspace>> GetWorkspacesAsync()
        {
            Uri requestUri = GetApiUri();
            return await Api.GetJsonObjectAsync<IList<HyperthoughtWorkspace>>(requestUri);
        }
        /// <summary>
        /// Obtains a HyperThought workspace specified by a unique identifier.
        /// </summary>
        /// <param name="workspaceId">The unique identifier of the HyperThought workspace.</param>
        /// <returns>The workspace obtained from the HyperThought API.</returns>
        public async Task<HyperthoughtWorkspace> GetWorkspaceAsync(Guid workspaceId)
        {
            Uri requestUri = GetWorkspaceUri(workspaceId);
            return await Api.GetJsonObjectAsync<HyperthoughtWorkspace>(requestUri);
        }
        
        /// <summary>
        /// Obtains the list of HyperThought users that have access to a workspace specified by a unique identifier.
        /// </summary>
        /// <param name="workspaceId">The workspace specified by a unique identifier.</param>
        /// <returns>The list of users obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtUserReference>> GetWorkspaceUsersAsync(Guid workspaceId)
        {
            Uri requestUri = new(GetWorkspaceUri(workspaceId), "/users");
            return await Api.GetJsonObjectAsync<IList<HyperthoughtUserReference>>(requestUri);
        }
    }
}