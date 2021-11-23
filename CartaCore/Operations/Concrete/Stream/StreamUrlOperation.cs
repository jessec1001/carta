using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

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
        public Stream Stream { get; set; }
    }

    /// <summary>
    /// Retrieves a stream of data from a specified URL.
    /// </summary>
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