using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Serialization.Json;

namespace CartaCore.Integration.Hyperthought
{
    /// <summary>
    /// A simple connector API used to connect Carta to HyperThought.
    /// </summary>
    public class HyperthoughtApi
    {
        private HttpClientHandler ClientHandler;
        private HttpClient Client;
        private HyperthoughtApiAccess Access;

        private readonly JsonSerializerOptions JsonOptions;

        /// <summary>
        /// The base URL from which the HyperThought instance is running.
        /// </summary>
        public Uri GetBaseUri() =>
            new Uri(Access?.BaseUrl);
        /// <summary>
        /// The get projects endpoint URL at the HyperThought instance.
        /// </summary>
        protected Uri GetProjectsUri() =>
            new Uri($"{Access?.BaseUrl}/api/projects/project/");
        /// <summary>
        /// The get workflow templates endpoint URL at the HyperThought instance.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        protected Uri GetWorkflowTemplatesUri(Guid projectId) =>
            new Uri($"{Access?.BaseUrl}/api/workflow/process/templates/?project={projectId}");
        /// <summary>
        /// The get workflow endpoint URL at the HyperThought instance.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        protected Uri GetWorkflowUri(Guid workflowId) =>
            new Uri($"{Access?.BaseUrl}/api/workflow/process/{workflowId}/");
        /// <summary>
        /// The get workflow children endpoint URL at the HyperThought instance.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        protected Uri GetWorkflowChildrenUri(Guid workflowId) =>
            new Uri($"{Access?.BaseUrl}/api/workflow/process/{workflowId}/children/");

        /// <summary>
        /// Constructs an instance of the HyperThought API with a given API access key.
        /// </summary>
        /// <param name="apiKey">The API access key.</param>
        public HyperthoughtApi(string apiKey)
        {
            // Use the API key to perform authorization.
            try
            {
                byte[] apiJson = Convert.FromBase64String(apiKey);
                Access = JsonSerializer.Deserialize<HyperthoughtApiAccess>(apiJson);
            }
            catch (ArgumentNullException exception)
            {
                throw new HttpRequestException("No API key provided.", exception, HttpStatusCode.Unauthorized);
            }
            catch (FormatException exception)
            {
                throw new HttpRequestException("API key in invalid format.", exception, HttpStatusCode.Unauthorized);
            }
            catch (JsonException exception)
            {
                throw new HttpRequestException("API key in invalid format.", exception, HttpStatusCode.Unauthorized);
            }

            // Create the HTTP client.
            ClientHandler = new HttpClientHandler();
            Client = new HttpClient(ClientHandler);

            // Set the DOD cookie.
            CookieContainer cookies = new CookieContainer();
            cookies.Add(new Cookie("dodAccessBanner", "true") { Domain = GetBaseUri().Host });

            ClientHandler.CookieContainer = cookies;
            ClientHandler.AllowAutoRedirect = true;

            // Set the authorization header.
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Access?.AccessToken);

            // Set the JSON options.
            JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            JsonOptions.Converters.Insert(0, new NullableEmptyStringConverter());
        }

        /// <summary>
        /// Obtains the list of HyperThought projects that the HyperThought user has access to.
        /// </summary>
        /// <returns>The list of projects obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtProject>> GetProjectsAsync()
        {
            HttpResponseMessage response = await Client.GetAsync(GetProjectsUri());
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IList<HyperthoughtProject>>(JsonOptions);
        }

        /// <summary>
        /// Obtains the list of HyperThought workflow templates that exist within a HyperThought project specified by
        /// project ID.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>The list of workflow templates obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkflowTemplate>> GetWorkflowTemplatesAsync(Guid projectId)
        {
            HttpResponseMessage response = await Client.GetAsync(GetWorkflowTemplatesUri(projectId));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IList<HyperthoughtWorkflowTemplate>>(JsonOptions);
        }
        /// <summary>
        /// Obtains the list of HyperThought workflow templates that exist within a HyperThought project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns>The list of workflow templates obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkflowTemplate>> GetWorkflowTemplatesAsync(HyperthoughtProject project)
        {
            return await GetWorkflowTemplatesAsync(project.Content.PrimaryKey);
        }

        /// <summary>
        /// Obtains a HyperThought workflow specified by workflow ID.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <returns>The workflow obtained from the HyperThought API.</returns>
        public async Task<HyperthoughtWorkflow> GetWorkflowAsync(Guid workflowId)
        {
            HttpResponseMessage response = await Client.GetAsync(GetWorkflowUri(workflowId));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<HyperthoughtWorkflow>(JsonOptions);
        }

        /// <summary>
        /// Obtains the list of children HyperThought workflows that exist within a HyperThought template or workflow
        /// specified by workflow ID.
        /// </summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <returns>The list of children workflows obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkflow>> GetWorkflowChildrenAsync(Guid workflowId)
        {
            HttpResponseMessage response = await Client.GetAsync(GetWorkflowChildrenUri(workflowId));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IList<HyperthoughtWorkflow>>(JsonOptions);
        }
        /// <summary>
        /// Obtains the list of children HyperThought workflows that exist within a HyperThought template or workflow
        /// specified by the workflow template.
        /// </summary>
        /// <param name="template">The template workflow.</param>
        /// <returns>The list of children workflows obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkflow>> GetWorkflowChildrenAsync(HyperthoughtWorkflowTemplate template)
        {
            return await GetWorkflowChildrenAsync(template.PrimaryKey);
        }
        /// <summary>
        /// Obtains the list of children HyperThought workflows that exist within a HyperThought template or workflow
        /// specified by workflow.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        /// <returns>The list of children workflows obtained from the HyperThought API.</returns>
        public async Task<IList<HyperthoughtWorkflow>> GetWorkflowChildrenAsync(HyperthoughtWorkflow workflow)
        {
            return await GetWorkflowChildrenAsync(workflow.Content.PrimaryKey);
        }

        /// <summary>
        /// Obtains the UUID for a workflow specified by a dot-separated path.
        /// </summary>
        /// <param name="path">
        /// The dot-separated path in the form of <c>"Project.Template.WorkflowA.WorkflowB"</c> and so forth.
        /// </param>
        /// <returns>
        /// The UUID of the workflow specified by a path if it exists. Otherwise, returns <see cref="Guid.Empty"/>.
        /// </returns>
        public async Task<Guid> GetWorkflowIdFromPathAsync(string path)
        {
            // Get the parts of the path.
            if (path is null) return Guid.Empty;
            string[] pathParts = path.Split('.');

            // These path parts are required.
            string projectPath = pathParts.Length > 0 ? pathParts[0] : null;
            string templatePath = pathParts.Length > 1 ? pathParts[1] : null;
            if (projectPath is null || templatePath is null) return Guid.Empty;

            // Get the project.
            HyperthoughtProject project = (await GetProjectsAsync())
                .Where(project => project.Content.Title.ToLower() == projectPath.ToLower())
                .FirstOrDefault();
            if (project is null) return Guid.Empty;
            // Get the workflow template.
            HyperthoughtWorkflowTemplate template = (await GetWorkflowTemplatesAsync(project))
                .Where(template => template.Title.ToLower() == templatePath.ToLower())
                .FirstOrDefault();
            if (template is null) return Guid.Empty;

            // Get workflows from the remaining path parts.
            Guid uuid = template.PrimaryKey;
            for (int k = 2; k < pathParts.Length; k++)
            {
                HyperthoughtWorkflow workflow = (await GetWorkflowChildrenAsync(uuid))
                    .Where(workflow => workflow.Content.Name.ToLower() == pathParts[k].ToLower())
                    .FirstOrDefault();
                if (workflow is null) return Guid.Empty;
                uuid = workflow.Content.PrimaryKey;
            }
            return uuid;
        }
    }
}