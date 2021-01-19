using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace CartaWeb.Extended
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AcceptsAttribute : Attribute, IActionConstraint
    {
        protected MediaTypeCollection ContentTypes { get; set; }
        public int Order { get; set; }

        public AcceptsAttribute(params string[] contentTypes)
        {
            ContentTypes = new MediaTypeCollection();
            foreach (string contentType in contentTypes)
                ContentTypes.Add(contentType);
        }

        public bool Accept(ActionConstraintContext context)
        {
            StringValues acceptRequest = context.RouteContext.HttpContext.Request.Headers[HeaderNames.Accept];
            MediaType acceptType = new MediaType(acceptRequest);

            if (StringValues.IsNullOrEmpty(acceptRequest)) return true;
            if (ContentTypes.Any(contentType => acceptType.IsSubsetOf(new MediaType(contentType)))) return true;

            return false;
        }
    }
}