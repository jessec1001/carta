using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    /// <summary>
    /// The input for the <see cref="StreamUrlOperation" /> operation.
    /// </summary>
    public struct StreamUrlOperationIn
    {
        /// <summary>
        /// The URL to request a data stream from.
        /// </summary>
        [FieldRequired]
        [FieldName("URL")]
        public string Url { get; set; }
    }
    /// <summary>
    /// The output for the <see cref="StreamUrlOperation" /> operation.
    /// </summary>
    public struct StreamUrlOperationOut
    {
        /// <summary>
        /// The stream of information retrieved from the URL.
        /// </summary>
        [FieldName("Stream")]
        public Stream Stream { get; set; }
    }

    /// <summary>
    /// Retrieves a stream of data from a specified URL.
    /// </summary>
    [OperationName(Display = "Stream Data from URL", Type = "streamUrl")]
    [OperationTag(OperationTags.Loading)]
    public class StreamUrlOperation : TypedOperation
    <
        StreamUrlOperationIn,
        StreamUrlOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<StreamUrlOperationOut> Perform(StreamUrlOperationIn input)
        {
            // Simply return a stream directed at the URL.
            HttpClient httpClient = new();
            Stream httpStream = await httpClient.GetStreamAsync(input.Url);

            return new StreamUrlOperationOut() { Stream = httpStream };
        }
    }
}