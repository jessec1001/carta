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
using CartaCore.Graphs;
using CartaWeb.Serialization.Json;
using CartaWeb.Serialization.Xml;

namespace CartaWeb.Formatters
{
    /// <summary>
    /// Represents a function that transforms a graph into a string.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <returns>The formatted string representation.</returns>
    public delegate Task<string> MediaFormatter(IGraph graph);

    /// <summary>
    /// Represents a text output formatter that is able to perform content negotiation and formatting for graphs.
    /// </summary>
    public class GraphOutputFormatter : TextOutputFormatter
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            IgnoreNullValues = true
        };

        private static readonly List<(MediaTypeHeaderValue MediaHeader, MediaFormatter Formatter)> MediaFormatters = new()
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
            foreach (var (MediaHeader, _) in MediaFormatters)
                SupportedMediaTypes.Add(MediaHeader);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        protected override bool CanWriteType(Type type)
        {
            if (typeof(IGraph).IsAssignableFrom(type))
                return base.CanWriteType(type);
            return false;
        }

        /// <inheritdoc />
        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            MediaTypeHeaderValue contentHeader = MediaTypeHeaderValue.Parse(context.ContentType);

            // Find the correct formatter and use it to write the content.
            string content = string.Empty;
            if (context.Object is IGraph graph)
            {
                foreach (var (MediaHeader, Formatter) in MediaFormatters)
                {
                    if (contentHeader.IsSubsetOf(MediaHeader))
                    {
                        content = await Formatter(graph);
                        break;
                    }
                }
            }

            // Write out the content to the response.
            await context.HttpContext.Response.WriteAsync(content, selectedEncoding);
        }

        private static string SerializeJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, JsonOptions);
        }
        private static string SerializeXml<T>(T obj)
        {
            using StringWriter sw = new();
            using (XmlWriter xw = XmlWriter.Create(sw))
            {
                XmlSerializer serializer = new(typeof(T));
                serializer.Serialize(xw, obj);
            }
            return sw.ToString();
        }

        private static async Task<string> FormatJg(IGraph graph) => SerializeJson(await JgFormat.CreateAsync(graph));
        private static async Task<string> FormatVis(IGraph graph) => SerializeJson(await VisFormat.CreateAsync(graph));
        private static async Task<string> FormatGex(IGraph graph) => SerializeXml(await GexFormat.CreateAsync(graph));
    }
}