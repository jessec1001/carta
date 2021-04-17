using System.Collections.Generic;

namespace CartaCore.Data
{
    public interface IRootedGraph : IGraph
    {
        IEnumerable<Identity> GetRoots();
    }
}