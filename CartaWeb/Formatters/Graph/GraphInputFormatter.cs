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

using CartaCore.Data;
using CartaWeb.Serialization.Json;
using CartaWeb.Serialization.Xml;

namespace CartaWeb.Formatters
{
    /// <summary>
    /// Represents a function that transforms a text stream into a graph.
    /// </summary>
    /// <param name="stream">The text stream.</param>
    /// <returns>The unformatted graph representation.</returns>
    public delegate Task<IEntireGraph> MediaUnformatter(Stream stream);

    /// <summary>
    /// Represents a text input formatter that is able to perform content negotiation and formatting for graphs.
    /// </summary>
    public class GraphInputFormatter : TextInputFormatter
    {
        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        private static List<(MediaTypeHeaderValue MediaHeader, MediaUnformatter Formatter)> MediaUnformatters
            = new List<(MediaTypeHeaderValue, MediaUnformatter)>()
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
            foreach (var unformatter in MediaUnformatters)
                SupportedMediaTypes.Add(unformatter.MediaHeader);

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        /// <inheritdoc />
        protected override bool CanReadType(Type type)
        {
            if (typeof(IEntireGraph).IsAssignableFrom(type))
                return base.CanReadType(type);
            return false;
        }

        /// <inheritdoc />
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            MediaTypeHeaderValue contentHeader = MediaTypeHeaderValue.Parse(context.HttpContext.Request.ContentType);
            Stream stream = context.HttpContext.Request.Body;

            // Find the correct unformatter and use it to read the content.
            IEntireGraph graph = null;
            if (context.ModelType.IsAssignableTo(typeof(IEntireGraph)))
            {
                foreach (var unformatter in MediaUnformatters)
                {
                    if (contentHeader.IsSubsetOf(unformatter.MediaHeader))
                    {
                        graph = await unformatter.Formatter(stream);
                    }
                }
            }

            // Return the successfull input result.
            return await InputFormatterResult.SuccessAsync(graph);
        }

        private static async Task<T> DeserializeJson<T>(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        }
        private static async Task<T> DeserializeXml<T>(Stream stream)
        {
            using (XmlReader xr = XmlReader.Create(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return await Task.FromResult((T)serializer.Deserialize(xr));
            }
        }

        private static async Task<IEntireGraph> UnformatJg(Stream stream) => (await DeserializeJson<JgFormat>(stream)).Graph;
        private static async Task<IEntireGraph> UnformatVis(Stream stream) => (await DeserializeJson<VisFormat>(stream)).Graph;
        private static async Task<IEntireGraph> UnformatGex(Stream stream) => (await DeserializeXml<GexFormat>(stream)).Graph;
    }
}