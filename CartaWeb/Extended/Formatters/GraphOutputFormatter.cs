using System;
using System.IO;
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
            if (typeof(FreeformGraph).IsAssignableFrom(type))
                return base.CanWriteType(type);
            return false;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            FreeformGraph graph = (FreeformGraph)context.Object;

            StringSegment acceptSubtype = MediaTypeHeaderValue.Parse(context.ContentType).SubTypeWithoutSuffix;
            string content = string.Empty;
            if (acceptSubtype.Equals("json") || acceptSubtype.Equals("jgf")) content = FormatJgf(graph);
            // if (acceptSubtype.Equals("xml") || acceptSubtype.Equals("gexf")) content = FormatGexf(graph);

            return context.HttpContext.Response.WriteAsync(content, selectedEncoding);
        }

        private static string FormatJgf(FreeformGraph graph)
        {
            return JsonSerializer.Serialize<Jgf>(new Jgf(graph));
        }
        // private static string FormatGexf(FreeformGraph graph)
        // {
        //     using (StringWriter sw = new StringWriter())
        //     {
        //         using (XmlWriter xw = XmlWriter.Create(sw))
        //         {
        //             XmlSerializer serializer = new XmlSerializer(typeof(Gexf));
        //             serializer.Serialize(xw, new Gexf(graph));
        //         }
        //         return sw.ToString();
        //     }
        // }
    }
}