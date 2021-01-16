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
        private string ResolveRoute(Type type, RouteAttribute attr)
        {
            // We initialize the route to be the same as the template in the case of a literal.
            string route = attr.Template.ToLower();

            // Replace [controller] with name of controller.
            // If a name was specified on the attribute, use that.
            // Otherwise, extract the controller name from the type name.
            string controllerName = attr.Name ?? ResolveControllerName(type);
            route = route.Replace("[controller]", controllerName);

            // For now, we are only concerned with replacing the [controller] component.

            return route;
        }
        private string ResolveRoute(MethodInfo info, HttpMethodAttribute attr)
        {
            // Not yet implemented.
            return "";
        }

        [Produces("application/json")]
        [HttpGet]
        public IDictionary<string, IList<ApiEndpoint>> Get()
        {
            IDictionary<string, IList<ApiEndpoint>> endpoints = new Dictionary<string, IList<ApiEndpoint>>();

            IList<Type> controllerTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.GetCustomAttributes<ApiControllerAttribute>().Any())
                .ToList();

            foreach (Type controllerType in controllerTypes)
            {
                string controllerName = ResolveControllerName(controllerType);
                string path = controllerType
                    .GetCustomAttributes<RouteAttribute>()
                    .Select(attr => ResolveRoute(controllerType, attr))
                    .FirstOrDefault();

                IList<ApiEndpoint> controllerEndpoints = new List<ApiEndpoint>();
                endpoints.Add(controllerName, controllerEndpoints);

                foreach (MethodInfo controllerMethod in controllerType.GetMethods())
                {
                    controllerMethod
                        .GetCustomAttributes<HttpMethodAttribute>()
                        .ToList()
                        .ForEach(attr => controllerEndpoints.Add(new ApiEndpoint
                        {
                            Method = attr.GetMethodType(),
                            Path = path
                        }
                        ));
                }
            }

            return endpoints;
        }
    }
}