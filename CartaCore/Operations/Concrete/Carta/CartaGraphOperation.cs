using System.Threading.Tasks;
using CartaCore.Data;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    public struct CartaGraphOperationIn
    {
        // TODO: Later, add in authentication requirement.
        public string Id { get; set; }
    }
    public struct CartaGraphOperationOut
    {
        public Graph Graph { get; set; }
    }

    /// <summary>
    /// Loads an existing graph uploaded to Carta.
    /// </summary>
    [OperationName(Display = "Carta Stored Graph", Type = "cartaGraph")]
    [OperationTag(OperationTags.Graph)]
    [OperationTag(OperationTags.Loading)]
    public class CartaGraphOperation : TypedOperation
    <
        CartaGraphOperationIn,
        CartaGraphOperationOut
    >
    {
        /// <inheritdoc />
        public override async Task<CartaGraphOperationOut> Perform(CartaGraphOperationIn input)
        {
            // TODO: We need to call the Carta API in order to fetch the user defined graph.
            //       This will require an integration with the Carta API itself with its own authentication forwarding.
            return new CartaGraphOperationOut()
            {
                Graph = null
            };
        }
    }
}