using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using CartaCore.Utilities;

namespace CartaCore.Documentation
{
    /// <summary>
    /// A possible return status subcomponent of an endpoint.
    /// </summary>
    public class EndpointReturn
    {
        private string _status;
        private string _return;

        /// <summary>
        /// The numeric representation of the status code.
        /// </summary>
        [XmlIgnore]
        public int StatusCode
        {
            get
            {
                _ = int.TryParse(_status, out int code);
                return code;
            }
        }
        /// <summary>
        /// The status code as parsed.
        /// </summary>
        [XmlAttribute(AttributeName = "status")]
        public string Status
        {
            get => _status.ContractWhitespace();
            set => _status = value;
        }

        /// <summary>
        /// The description of the return value and conditions.
        /// </summary>
        [XmlText]
        public string Return
        {
            get => _return.ContractWhitespace();
            set => _return = value;
        }
    }

    /// <summary>
    /// A sample request that contains information necessary to make a valid request to the endpoint.
    /// </summary>
    public class EndpointRequest
    {
        /// <summary>
        /// The name of the request.
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The arguments of the request as a dictionary representation.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, string> Parameters
        {
            get
            {
                return Arguments.ToDictionary(
                    arg => arg.Name,
                    arg => arg.Value
                );
            }
        }
        /// <summary>
        /// The arguments of the request.
        /// </summary>
        [XmlElement(ElementName = "arg")]
        public EndpointParameter[] Arguments { get; set; }

        /// <summary>
        /// The JSON representation of the body of the request.
        /// </summary>
        [XmlIgnore]
        public JsonDocument JsonBody
        {
            get => JsonSerializer.Deserialize<JsonDocument>(
                Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
            );
        }
        /// <summary>
        /// A string representation of a JSON object that should be sent as the body of the request.
        /// </summary>
        [XmlElement(ElementName = "body")]
        public string Body { get; set; }
    }

    /// <summary>
    /// A parameter subcomponent for an endpoint signature or sample endpoint request.
    /// </summary>
    public class EndpointParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The description or value of the parameter.
        /// </summary>
        [XmlText]
        public string Value { get; set; }
    }

    /// <summary>
    /// The extended set of documentation for an endpoint.
    /// </summary>
    public class EndpointDocumentation : StandardDocumentation
    {
        /// <summary>
        /// The parameters for the endpoint.
        /// </summary>
        [XmlElement(ElementName = "param")]
        public EndpointParameter[] Parameters { get; set; }
        /// <summary>
        /// The possible return values for the endpoint.
        /// </summary>
        [XmlElement(ElementName = "returns")]
        public EndpointReturn[] Returns { get; set; }

        /// <summary>
        /// The example requests for the endpoint.
        /// </summary>
        [XmlElement(ElementName = "request")]
        public EndpointRequest[] Requests { get; set; }
    }
}