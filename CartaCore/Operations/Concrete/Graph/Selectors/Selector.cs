using System.Threading.Tasks;
using CartaCore.Data;

namespace CartaCore.Operations
{
    public class Selector
    {
        private Operation SelectorOperation { get; init; }

        public Selector(Operation operation)
        {
            SelectorOperation = operation;
        }

        public async Task<Graph> Select(Graph graph, object selectorInput)
        {
            return await Operation.ExecuteSelector(SelectorOperation, selectorInput, graph);
        }
    }
}