using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the functionality of the Projects module of the HyperThought API.
    /// </summary>
    public class HyperthoughtProjectsApi
    {
        private HyperthoughtApi Api { get; init; }

        /// <summary>
        /// Gets the projects API URI at the HyperThought instance.
        /// </summary>
        protected Uri GetApiUri() => new Uri(Api.GetBaseUri(), "projects/");

        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtProjectsApi"/> class with the specified base API.
        /// </summary>
        /// <param name="api">The base HyperThought API.</param>
        public HyperthoughtProjectsApi(HyperthoughtApi api)
        {
            Api = api;
        }

        /// <summary>
        /// Obtains the list of HyperThought projects that the HyperThought user has access to.
        /// </summary>
        /// <returns>The list of projects obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtProject>> GetProjectsAsync()
        {
            Uri requestUri = new Uri(GetApiUri(), "project/");
            return await Api.GetJsonAsync<IList<HyperthoughtProject>>(requestUri);
        }
    }
}