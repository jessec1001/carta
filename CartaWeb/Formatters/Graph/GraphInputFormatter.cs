using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using CartaCore.Graphs;
using CartaWeb.Serialization.Json;
using CartaWeb.Serialization.Xml;
using CartaCore.Graphs.Components;

namespace CartaWeb.Formatters
{
    /// <summary>
    /// Represents a function that transforms a text stream into a graph.
    /// </summary>
    /// <param name="stream">The text stream.</param>
    /// <returns>The unformatted graph representation.</returns>
    public delegate Task<IEnumerableComponent<Vertex, Edge>> MediaUnformatter(Stream stream);

    /// <summary>
    /// Represents a text input formatter that is able to perform content negotiation and formatting for graphs.
    /// </summary>
    public class GraphInputFormatter : TextInputFormatter
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            IgnoreNullValues = true
        };

        private static readonly List<(MediaTypeHeaderValue MediaHeader, MediaUnformatter Formatter)> MediaUnformatters = new()
        {
            // JSON-based formats.
            (MediaTypeHeaderValue.Parse("application/vnd.vis+json"), UnformatVis),
            (MediaTypeHeaderValue.Parse("application/vnd.jgf+json"), UnformatJg),
            (MediaTypeHeaderValue.Parse("application/json"), UnformatVis), // Default

            // XML-based formats.
            (MediaTypeHeaderValue.Parse("application/vnd.gexf+xml"), UnformatGex),
            (MediaTypeHeaderValue.Parse("application/xml"), UnformatGex), // Default
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphInputFormatter"/> class.
        /// </summary>
        public GraphInputFormatter()
        {
            foreach (var (MediaHeader, _) in MediaUnformatters)
                SupportedMediaTypes.Add(MediaHeader);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        protected override bool CanReadType(Type type)
        {
            if (typeof(IEnumerableComponent<IVertex, IEdge>).IsAssignableFrom(type))
                return base.CanReadType(type);
            return false;
        }

        /// <inheritdoc />
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            MediaTypeHeaderValue contentHeader = MediaTypeHeaderValue.Parse(context.HttpContext.Request.ContentType);
            Stream stream = context.HttpContext.Request.Body;

            // Find the correct unformatter and use it to read the content.
            IEnumerableComponent<IVertex, IEdge> graph = null;
            if (context.ModelType.IsAssignableTo(typeof(IEnumerableComponent<IVertex, IEdge>)))
            {
                foreach (var (MediaHeader, Formatter) in MediaUnformatters)
                {
                    if (contentHeader.IsSubsetOf(MediaHeader))
                    {
                        // Return the successful input result.
                        graph = await Formatter(stream);
                        return await InputFormatterResult.SuccessAsync(graph);
                    }
                }
            }

            // We did not find a matching format so return failure.
            return await InputFormatterResult.FailureAsync();
        }

        private static async Task<T> DeserializeJson<T>(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        }
        private static async Task<T> DeserializeXml<T>(Stream stream)
        {
            using XmlReader xr = XmlReader.Create(stream);
            XmlSerializer serializer = new(typeof(T));
            return await Task.FromResult((T)serializer.Deserialize(xr));
        }

        private static async Task<IEnumerableComponent<IVertex, IEdge>> UnformatJg(Stream stream) => (await DeserializeJson<JgFormat>(stream)).Graph;
        private static async Task<IEnumerableComponent<IVertex, IEdge>> UnformatVis(Stream stream) => (await DeserializeJson<VisFormat>(stream)).Graph;
        private static async Task<IEnumerableComponent<IVertex, IEdge>> UnformatGex(Stream stream) => (await DeserializeXml<GexFormat>(stream)).Graph;
    }
}