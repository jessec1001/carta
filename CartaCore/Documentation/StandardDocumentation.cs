using System.Xml.Serialization;
using CartaCore.Extensions.String;

namespace CartaCore.Documentation
{
    /// <summary>
    /// The standard set of documentation element values for a member.
    /// </summary>
    [XmlRoot("member")]
    public class StandardDocumentation
    {
        private string _summary;

        /// <summary>
        /// The summary of the member.
        /// </summary>
        [XmlElement(ElementName = "summary")]
        public string Summary
        {
            get => _summary.ContractWhitespace();
            set => _summary = value;
        }
        /// <summary>
        /// The summary of the member without trimming applied.
        /// </summary>
        [XmlIgnore]
        public string UncontractedSummary
        {
            get => _summary.Trim();
            set => _summary = value;
        }
    }
}