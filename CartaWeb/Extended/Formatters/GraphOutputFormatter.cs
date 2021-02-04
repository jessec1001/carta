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
using CartaWeb.Serialization.Json;
using CartaWeb.Serialization.Xml;

namespace CartaWeb.Extended.Formatters
{
    using FreeformGraph = IMutableVertexAndEdgeSet<FreeformVertex, FreeformEdge>;

    /// <summary>
    /// Represents a function that transforms a freeform graph into a string.
    /// </summary>
    /// <param name="graph">The freeform graph.</param>
    /// <returns>The formatted string representation.</returns>
    public delegate string MediaFormatter(FreeformGraph graph);

    /// <summary>
    /// Represents a text output formatter that is able to perform content negotiation and formatting for freeform
    /// graphs.
    /// </summary>
    public class GraphOutputFormatter : TextOutputFormatter
    {
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        private static Dictionary<MediaTypeHeaderValue, MediaFormatter> MediaFormatters
            = new Dictionary<MediaTypeHeaderValue, MediaFormatter>()
            {
                // JSON-based formats.
                [MediaTypeHeaderValue.Parse("application/json")] = FormatVis,
                [MediaTypeHeaderValue.Parse("application/vnd.vis+json")] = FormatVis,
                [MediaTypeHeaderValue.Parse("application/vnd.jgf+json")] = FormatJg,

                // XML-based formats.
                [MediaTypeHeaderValue.Parse("application/xml")] = FormatGex,
                [MediaTypeHeaderValue.Parse("application/vnd.gexf+xml")] = FormatGex
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOutputFormatter"/> class.
        /// </summary>
        public GraphOutputFormatter()
        {
            foreach (MediaTypeHeaderValue header in MediaFormatters.Keys)
                SupportedMediaTypes.Add(header);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        protected override bool CanWriteType(Type type)
        {
            if (typeof(FreeformGraph).IsAssignableFrom(type))
                return base.CanWriteType(type);
            return false;
        }

        /// <inheritdoc />
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            MediaTypeHeaderValue contentHeader = MediaTypeHeaderValue.Parse(context.ContentType);

            // Find the correct formatter and use it to write the content.
            string content = string.Empty;
            if (context.Object is FreeformGraph graph)
            {
                foreach (KeyValuePair<MediaTypeHeaderValue, MediaFormatter> pair in MediaFormatters)
                {
                    if (contentHeader.IsSubsetOf(pair.Key))
                    {
                        content = pair.Value(graph);
                        break;
                    }
                }
            }

            // Write out the content to the response.
            return context.HttpContext.Response.WriteAsync(content, selectedEncoding);
        }

        private static string FormatJson<T>(T obj)
        {
            return JsonSerializer.Serialize<T>(obj, JsonOptions);
        }
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

        private static string FormatJg(FreeformGraph graph) => FormatJson<JgFormat>(new JgFormat(graph));
        private static string FormatVis(FreeformGraph graph) => FormatJson<VisFormat>(new VisFormat(graph));
        private static string FormatGex(FreeformGraph graph) => FormatXml<GexFormat>(new GexFormat(graph));
    }
}