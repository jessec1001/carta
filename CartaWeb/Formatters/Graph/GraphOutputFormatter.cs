using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

using CartaCore.Data;
using CartaCore.Workflow.Selection;
using CartaWeb.Serialization.Json;
using CartaWeb.Serialization.Xml;

namespace CartaWeb.Formatters
{
    /// <summary>
    /// Represents a function that transforms a graph into a string.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <returns>The formatted string representation.</returns>
    public delegate Task<string> MediaFormatter(IEntireGraph graph);

    /// <summary>
    /// Represents a text output formatter that is able to perform content negotiation and formatting for graphs.
    /// </summary>
    public class GraphOutputFormatter : TextOutputFormatter
    {
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        private static List<(MediaTypeHeaderValue MediaHeader, MediaFormatter Formatter)> MediaFormatters
            = new List<(MediaTypeHeaderValue, MediaFormatter)>()
            {
                // JSON-based formats.
                (MediaTypeHeaderValue.Parse("application/vnd.vis+json"), FormatVis),
                (MediaTypeHeaderValue.Parse("application/vnd.jgf+json"), FormatJg),
                (MediaTypeHeaderValue.Parse("application/json"), FormatVis), // Default

                // XML-based formats.
                (MediaTypeHeaderValue.Parse("application/vnd.gexf+xml"), FormatGex),
                (MediaTypeHeaderValue.Parse("application/xml"), FormatGex), // Default
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOutputFormatter"/> class.
        /// </summary>
        public GraphOutputFormatter()
        {
            foreach (var formatter in MediaFormatters)
                SupportedMediaTypes.Add(formatter.MediaHeader);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        protected override bool CanWriteType(Type type)
        {
            if (typeof(Selector).IsAssignableFrom(type))
                return false;
            if (typeof(IEntireGraph).IsAssignableFrom(type))
                return base.CanWriteType(type);
            return false;
        }

        /// <inheritdoc />
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            MediaTypeHeaderValue contentHeader = MediaTypeHeaderValue.Parse(context.ContentType);

            // Find the correct formatter and use it to write the content.
            string content = string.Empty;
            if (context.Object is IEntireGraph graph)
            {
                foreach (var formatter in MediaFormatters)
                {
                    if (contentHeader.IsSubsetOf(formatter.MediaHeader))
                    {
                        content = await formatter.Formatter(graph);
                        break;
                    }
                }
            }

            // Write out the content to the response.
            await context.HttpContext.Response.WriteAsync(content, selectedEncoding);
        }

        private static string SerializeJson<T>(T obj)
        {
            return JsonSerializer.Serialize<T>(obj, JsonOptions);
        }
        private static string SerializeXml<T>(T obj)
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

        private static async Task<string> FormatJg(IEntireGraph graph) => SerializeJson<JgFormat>(await JgFormat.CreateAsync(graph));
        private static async Task<string> FormatVis(IEntireGraph graph) => SerializeJson<VisFormat>(await VisFormat.CreateAsync(graph));
        private static async Task<string> FormatGex(IEntireGraph graph) => SerializeXml<GexFormat>(await GexFormat.CreateAsync(graph));
    }
}