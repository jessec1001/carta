using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using QuikGraph;

using CartaCore.Data;
using CartaCore.Serialization.Json.Jgf;
using CartaCore.Serialization.Xml.Gexf;

namespace CartaWeb.Extended.Formatters
{
    using FreeformGraph = IEdgeListAndIncidenceGraph<FreeformVertex, Edge<FreeformVertex>>;

    public class GraphOutputFormatter : TextOutputFormatter
    {
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        public GraphOutputFormatter()
        {
            foreach (string mediaType in new string[]
                {
                    "application/json", "text/json",
                    "application/jgf", "text/jgf",
                    "application/xml", "text/xml",
                    "application/gexf", "text/gexf"
                }
            ) SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(FreeformGraph).IsAssignableFrom(type) ||
                typeof(FreeformVertex).IsAssignableFrom(type) ||
                typeof(IEnumerable<FreeformVertex>).IsAssignableFrom(type) ||
                typeof(IDictionary<Guid, FreeformVertex>).IsAssignableFrom(type))
                return base.CanWriteType(type);
            return false;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            StringSegment acceptSubtype = MediaTypeHeaderValue.Parse(context.ContentType).SubTypeWithoutSuffix;
            string content = string.Empty;

            if (acceptSubtype.Equals("json") || acceptSubtype.Equals("jgf"))
            {
                switch (context.Object)
                {
                    case FreeformGraph graph:
                        content = FormatJgf(graph);
                        break;
                    case FreeformVertex vertex:
                        content = FormatJgf(vertex);
                        break;
                    case IEnumerable<FreeformVertex> vertices:
                        content = FormatJgf(vertices);
                        break;
                    case IDictionary<Guid, FreeformVertex> vertices:
                        content = FormatJgf(vertices);
                        break;
                }
            }

            return context.HttpContext.Response.WriteAsync(content, selectedEncoding);
        }

        private static string FormatJson<T>(T obj) =>
            JsonSerializer.Serialize<T>(obj, JsonOptions);
        private static string FormatJgf(FreeformGraph graph) =>
            FormatJson<Jgf>(new Jgf(graph));
        private static string FormatJgf(FreeformVertex vertex) =>
            FormatJson<JgfNode>(new JgfNode(vertex));
        private static string FormatJgf(IEnumerable<FreeformVertex> vertices) =>
            FormatJson<IEnumerable<JgfNode>>(vertices.Select(vertex => new JgfNode(vertex)));
        private static string FormatJgf(IDictionary<Guid, FreeformVertex> vertices) =>
            FormatJson<IDictionary<Guid, JgfNode>>(vertices.ToDictionary(
                pair => pair.Key,
                pair => new JgfNode(pair.Value)
            ));

        private static string FormatXml<T>(T obj)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter xw = XmlWriter.Create(sw))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(xw, obj);
                }
                return sw.ToString();
            }
        }
    }
}