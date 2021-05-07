using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

using Json.Schema;
using Json.Schema.Generation;

using CartaCore.Serialization;
using CartaCore.Workflow.Action;
using CartaCore.Workflow.Selection;
using CartaWeb.Models.Meta;

namespace CartaWeb.Controllers
{
    /// <summary>
    /// Serves information about active API endpoints accessible by the server. Each endpoint is uniquely specified by
    /// the HTTP method and the endpoint path. Additional information is provided on the schema of the API, default
    /// values, descriptions of endpoints and parameters, return values, and example requests.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MetaController : ControllerBase
    {
        /// <summary>
        /// The logger for this controller.
        /// </summary>
        private readonly ILogger<MetaController> _logger;

        /// <inheritdoc />
        public MetaController(ILogger<MetaController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Resolves the string name of a controller type.
        /// </summary>
        /// <remarks>
        /// If the type name does not end in <c>"Controller"</c>, then, the entire type name is assumed to be the
        /// controller name. Otherwise, only the prefix is considered to be the controller name.
        /// </remarks>
        /// <param name="type">The controller type.</param>
        /// <returns>A string representing the name of the controller.</returns>
        private string ResolveControllerName(Type type)
        {
            // If the type ends in "Controller" (which it should), remove it and set the remainder as the type name.
            const string suffix = "controller";
            string typeName = type.Name.ToLower();
            if (typeName.EndsWith(suffix))
                return typeName.Substring(0, typeName.Length - suffix.Length);
            else
                return typeName;
        }

        /// <summary>
        /// Resolves the list of routes to a controller type.
        /// </summary>
        /// <param name="type">The controller type.</param>
        /// <returns>An enumerable of string routes.</returns>
        private IEnumerable<string> ResolveControllerRoute(Type type)
        {
            // Get the name of the type as a controller.
            string controllerName = ResolveControllerName(type);

            // Iterate over each route attribute as a separate route.
            foreach (RouteAttribute attr in type.GetCustomAttributes<RouteAttribute>())
            {
                // We initialize the route to be the same as the template in the case of a literal.
                string route = (attr.Template ?? "[controller]")
                    .ToLower()
                    .Trim('/');

                // If a name was specified on the attribute, use that.
                // Otherwise, extract the controller name from the type name.
                string replacementName = attr.Name ?? controllerName;

                // Replace [controller] with name of controller.
                route = route.Replace("[controller]", replacementName);

                // For now, we are only concerned with replacing the [controller] component.
                yield return route;
            }
        }
        /// <summary>
        /// Resolves the list of endpoints to a controller action.
        /// </summary>
        /// <param name="method">The controller action.</param>
        /// <returns>An enumerable of API endpoints.</returns>
        private IEnumerable<ApiEndpoint> ResolveActionRoute(MethodInfo method)
        {
            // Get the containing controller routes.
            IEnumerable<string> controllerRoutes = ResolveControllerRoute(method.DeclaringType);

            IEnumerable<IRouteTemplateProvider> httpAttrs = method.GetCustomAttributes<HttpMethodAttribute>();
            IEnumerable<IRouteTemplateProvider> routeAttrs = method.GetCustomAttributes<RouteAttribute>();
            foreach (IRouteTemplateProvider attr in httpAttrs.Union(routeAttrs))
            {
                // We initialize the route to be the same as the template in the case of a literal.
                string route = (attr.Template ?? string.Empty)
                    .ToLower()
                    .Trim('/');

                // We add the query string, if any, to the action route.
                string queryString = string.Join('&', method
                        .GetParameters()
                        .Where(param => Nullable.GetUnderlyingType(param.ParameterType) is null)
                        .Select(param => ResolveParameterString(param))
                        .Where(paramStr => !string.IsNullOrEmpty(paramStr))
                );
                route = string.IsNullOrEmpty(queryString) ? route : $"{route}?{queryString}";
                List<ApiParameter> parameters = ResolveActionParameters(method);

                // The complete route is the concatenation of the controller and action route.
                foreach (string controllerRoute in controllerRoutes)
                {
                    ApiMethod actionMethod = default(ApiMethod);
                    string actionRoute;
                    if (string.IsNullOrEmpty(route))
                        actionRoute = controllerRoute;
                    else
                        actionRoute = string.Join('/', controllerRoute, route);

                    // If the attribute was an HTTP method, use it as the method type.
                    if (attr is HttpMethodAttribute httpAttr)
                        actionMethod = httpAttr.GetMethodType();

                    yield return new ApiEndpoint
                    {
                        Method = actionMethod,
                        Path = actionRoute,
                        Parameters = parameters,
                        Requests = method.GetDocsRequests(),
                        Description = method.GetDocsSummary(),
                        Returns = method.GetDocsReturns(),
                    };
                }
            }
        }
        /// <summary>
        /// Resolves the list of parameters to a controller action.
        /// </summary>
        /// <param name="method">The controller action.</param>
        /// <returns>A list of API parameters.</returns>
        private List<ApiParameter> ResolveActionParameters(MethodInfo method)
        {
            // We get the parameter list.
            List<ApiParameter> parameters =
                method
                    .GetParameters()
                    .Select(param =>
                    {
                        // We check for a custom name specified by its query name.
                        string name = param.GetCustomAttribute<FromQueryAttribute>()?.Name ?? param.Name;

                        // We get the type of the parameter.
                        // We check for a different underlying type using our meta attribute.
                        // We also need to check for nullability.
                        Type parameterType = param.ParameterType;
                        parameterType = parameterType.GetCustomAttribute<ApiTypeAttribute>()?.UnderlyingType ?? parameterType;

                        // Figure out the parameter receiving format.
                        ApiParameterFormat format = ApiParameterFormat.Query;
                        if (param.GetCustomAttribute<FromRouteAttribute>() is not null)
                            format = ApiParameterFormat.Route;
                        if (param.GetCustomAttribute<FromQueryAttribute>() is not null)
                            format = ApiParameterFormat.Query;
                        if (param.GetCustomAttribute<FromBodyAttribute>() is not null)
                            format = ApiParameterFormat.Body;

                        return new ApiParameter
                        {
                            Name = name,
                            Type = parameterType,
                            Format = format,
                            Description = param.GetDocsSummary()
                        };
                    })
                    .ToList();
            return parameters;
        }
        /// <summary>
        /// Resolves a parameter to a query string component.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The query string parameter.</returns>
        private string ResolveParameterString(ParameterInfo parameter)
        {
            // The parameter name is either set by the from query attribute or defaults to the name of the parameter.
            string parameterName = parameter
                .GetCustomAttributes<FromQueryAttribute>()
                .Select(attr => attr.Name ?? parameter.Name)
                .FirstOrDefault();

            // The parameter value is only set if the parameter has a default value.
            string parameterValue = parameter.HasDefaultValue
                ? (parameter.DefaultValue is null ? "null" : parameter.DefaultValue.ToString())
                : null;

            // If there is no default value, use 'param=value'; otherwise, use 'param'.
            return (parameterValue is null) ? parameterName : $"{parameterName}={parameterValue}";
        }

        /// <summary>
        /// Gets the list of all active API endpoints and associated documentation. The information provided should be
        /// sufficient to construct a request to one of the endpoints. Note that the documentation is updated when the
        /// application is built and deployed.
        /// </summary>
        /// <request name="Example"></request>
        /// <returns status="200">
        /// A list of API endpoints grouped together into collections of related functionality.
        /// </returns>
        [HttpGet]
        public List<ApiCollection> GetEndpoints()
        {
            List<ApiCollection> endpoints = new List<ApiCollection>();

            // Get each controller type in this assembly.
            IList<Type> controllerTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.GetCustomAttributes<ApiControllerAttribute>().Any())
                .ToList();

            // Iterate over each controller forming a category of endpoints.
            foreach (Type controllerType in controllerTypes)
            {
                // We add a list of endpoints per controller.
                string controllerName = ResolveControllerName(controllerType);
                List<ApiEndpoint> controllerEndpoints = new List<ApiEndpoint>();
                endpoints.Add
                (
                    new ApiCollection
                    {
                        Name = controllerName,
                        Endpoints = controllerEndpoints,
                        Description = controllerType.GetDocsSummary()
                    }
                );

                // We resolve the routes to each individual method and add it to the results.
                foreach (MethodInfo actionMethod in controllerType.GetMethods())
                    controllerEndpoints.AddRange(ResolveActionRoute(actionMethod));
            }

            return endpoints;
        }

        [HttpGet("action/{action}.schema")]
        public ActionResult<JsonSchema> GetActionSchema(string action)
        {
            if (Discriminant.TryGetType<ActionBase>(action, out Type actionType))
            {
                JsonSchemaBuilder schemaBuilder = new JsonSchemaBuilder();
                JsonSchema schema = schemaBuilder.FromType(actionType).Build();

                return Ok(schema);
            }
            else return BadRequest();
        }

        [HttpGet("selector/{selector}.schema")]
        public ActionResult<JsonSchema> GetSelectorSchema(string selector)
        {
            if (Discriminant.TryGetType<Selector>(selector, out Type selectorType))
            {
                JsonSchemaBuilder schemaBuilder = new JsonSchemaBuilder();
                JsonSchema schema = schemaBuilder.FromType(selectorType).Build();

                return Ok(schema);
            }
            else return BadRequest();
        }
    }
}