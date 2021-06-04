using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;

namespace CartaCore.Integration.Hyperthought.Api
{
    /// <summary>
    /// Represents the functionality of the Common module of the HyperThought API.
    /// </summary>
    public class HyperthoughtCommonApi
    {
        private HyperthoughtApi Api { get; init; }

        /// <summary>
        /// Gets the common API URI at the HyperThought instance.
        /// </summary>
        protected Uri GetApiUri() => new Uri(Api.GetBaseUri(), "common/");

        /// <summary>
        /// Initializes an instance of the <see cref="HyperthoughtCommonApi"/> class with the specified base API.
        /// </summary>
        /// <param name="api">The base HyperThought API.</param>
        public HyperthoughtCommonApi(HyperthoughtApi api)
        {
            Api = api;
        }

        /// <summary>
        /// Obtains a list of common units available to HyperThought metadata properties. 
        /// </summary>
        /// <returns>The list of common units obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtUnit>> GetUnitsAsync()
        {
            Uri requestUri = new Uri(GetApiUri(), "units/");
            return await Api.GetJsonObjectAsync<IList<HyperthoughtUnit>>(requestUri);
        }
        /// <summary>
        /// Obtains a list of defined vocabulary available to HyperThought metadata properties.
        /// </summary>
        /// <returns>The list of common definitions obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtVocabularyDefinition>> GetVocabularyAsync()
        {
            Uri requestUri = new Uri(GetApiUri(), "afrl-vocab/");
            return await Api.GetJsonObjectAsync<IList<HyperthoughtVocabularyDefinition>>(requestUri);
        }
        /// <summary>
        /// Obtains a list of basic user information for all HyperThought users.
        /// </summary>
        /// <returns>The list of users obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtUserReference>> GetUsersAsync()
        {
            Uri requestUri = new Uri(GetApiUri(), "user/");
            return await Api.GetJsonObjectAsync<IList<HyperthoughtUserReference>>(requestUri);
        }
    }
}