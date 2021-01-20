using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

using CartaWeb.Models.Meta;

namespace CartaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetaController : ControllerBase
    {
        private readonly ILogger<MetaController> _logger;

        public MetaController(ILogger<MetaController> logger)
        {
            _logger = logger;
        }

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
                        Path = actionRoute
                    };
                }
            }
        }

        [HttpGet]
        public IDictionary<string, IList<ApiEndpoint>> Get()
        {
            IDictionary<string, IList<ApiEndpoint>> endpoints = new Dictionary<string, IList<ApiEndpoint>>();

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
                endpoints.Add(controllerName, controllerEndpoints);

                // We resolve the routes to each individual method and add it to the results.
                foreach (MethodInfo actionMethod in controllerType.GetMethods())
                    controllerEndpoints.AddRange(ResolveActionRoute(actionMethod));
            }

            return endpoints;
        }
    }
}