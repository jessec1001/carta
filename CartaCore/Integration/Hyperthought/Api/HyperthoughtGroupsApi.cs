using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the functionality of the Groups module of the HyperThought API.
    /// </summary>
    public class HyperthoughtGroupsApi
    {
        private HyperthoughtApi Api { get; init; }

        /// <summary>
        /// Gets the groups API URI at the HyperThought instance.
        /// </summary>
        protected Uri GetApiUri() => new Uri(Api.GetBaseUri(), "groups/");

        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtGroupsApi"/> class with the specified base API.
        /// </summary>
        /// <param name="api">The base HyperThought API.</param>
        public HyperthoughtGroupsApi(HyperthoughtApi api)
        {
            Api = api;
        }

        /// <summary>
        /// Obtains the list of HyperThought groups that the HyperThought user has access to.
        /// </summary>
        /// <returns>The list of groups obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtGroup>> GetGroupsAsync()
        {
            Uri requestUri = GetApiUri();
            return await Api.GetJsonObjectAsync<IList<HyperthoughtGroup>>(requestUri);
        }
    }
}