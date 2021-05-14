using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MorseCode.ITask;

namespace CartaCore.Data
{
    public class FiniteGraph : Graph,
        IEntireGraph,
        IDynamicInGraph<InOutVertex>,
        IDynamicOutGraph<InOutVertex>
    {
        protected bool Directed { get; init; }

        public override bool IsDirected() => Directed;
        public override bool IsDynamic() => true;
        public override bool IsFinite() => true;

        private Dictionary<Identity, Vertex> VertexSet { get; init; }
        private Dictionary<Identity, HashSet<Edge>> InEdgeSet { get; init; }
        private Dictionary<Identity, HashSet<Edge>> OutEdgeSet { get; init; }

        public FiniteGraph(
            Identity id,
            IEnumerable<Property> properties,
            bool directed = true
        ) : base(id, properties)
        {
            Directed = directed;

            VertexSet = new Dictionary<Identity, Vertex>();
            InEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
            OutEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
        }
        public FiniteGraph(
            Identity id,
            bool directed = true
        ) : base(id)
        {
            Directed = directed;

            VertexSet = new Dictionary<Identity, Vertex>();
            InEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
            OutEdgeSet = new Dictionary<Identity, HashSet<Edge>>();
        }

        public bool AddVertex(IVertex vertex)
        {
            if (vertex is not Vertex vertexBase) return false;

            if (VertexSet.ContainsKey(vertex.Identifier))
                return false;
            else
            {
                VertexSet.Add(vertex.Identifier, vertexBase);
                if (vertex is IInVertex iVertex)
                    this.AddEdgeRange(iVertex.InEdges);
                if (vertex is IOutVertex oVertex)
                    this.AddEdgeRange(oVertex.OutEdges);
                return true;
            }
        }
        public int AddVertexRange(IEnumerable<IVertex> vertices)
        {
            if (vertices is null) throw new ArgumentNullException(nameof(vertices));

            int added = 0;
            foreach (IVertex vertex in vertices)
                if (AddVertex(vertex)) added++;
            return added;
        }
        public bool RemoveVertex(IVertex vertex)
        {
            return VertexSet.Remove(vertex.Identifier);
        }
        public int RemoveVertexRange(IEnumerable<IVertex> vertices)
        {
            if (vertices is null) throw new ArgumentNullException(nameof(vertices));

            int removed = 0;
            foreach (IVertex vertex in vertices)
                if (RemoveVertex(vertex)) removed++;
            return removed;
        }

        private bool AddInEdge(Identity vertex, Edge edge)
        {
            HashSet<Edge> edges;
            if (InEdgeSet.TryGetValue(vertex, out HashSet<Edge> existing))
                edges = existing;
            else
            {
                edges = new HashSet<Edge>();
                InEdgeSet.Add(vertex, edges);
            }
            return edges.Add(edge);
        }
        private bool AddOutEdge(Identity vertex, Edge edge)
        {
            HashSet<Edge> edges;
            if (OutEdgeSet.TryGetValue(vertex, out HashSet<Edge> existing))
                edges = existing;
            else
            {
                edges = new HashSet<Edge>();
                OutEdgeSet.Add(vertex, edges);
            }
            return edges.Add(edge);
        }
        private bool RemoveInEdge(Identity vertex, Edge edge)
        {
            if (InEdgeSet.TryGetValue(vertex, out HashSet<Edge> edges))
                return edges.Remove(edge);
            return false;
        }
        private bool RemoveOutEdge(Identity vertex, Edge edge)
        {
            if (OutEdgeSet.TryGetValue(vertex, out HashSet<Edge> edges))
                return edges.Remove(edge);
            return false;
        }

        public bool AddEdge(Edge edge)
        {
            if (IsDirected())
            {
                bool added = false;
                added = AddInEdge(edge.Target, edge) || added;
                added = AddOutEdge(edge.Source, edge) || added;
                return added;
            }
            else
            {
                bool added = false;
                added = AddInEdge(edge.Source, edge) || added;
                added = AddInEdge(edge.Target, edge) || added;
                added = AddOutEdge(edge.Source, edge) || added;
                added = AddOutEdge(edge.Target, edge) || added;
                return added;
            }
        }
        public int AddEdgeRange(IEnumerable<Edge> edges)
        {
            if (edges is null) throw new ArgumentNullException(nameof(edges));

            int added = 0;
            foreach (Edge edge in edges)
                if (AddEdge(edge)) added++;
            return added;
        }
        public bool RemoveEdge(Edge edge)
        {
            if (IsDirected())
            {
                bool removed = false;
                removed = RemoveInEdge(edge.Target, edge) || removed;
                removed = RemoveOutEdge(edge.Source, edge) || removed;
                return removed;
            }
            else
            {
                bool removed = false;
                removed = RemoveInEdge(edge.Source, edge) || removed;
                removed = RemoveInEdge(edge.Target, edge) || removed;
                removed = RemoveOutEdge(edge.Source, edge) || removed;
                removed = RemoveOutEdge(edge.Target, edge) || removed;
                return removed;
            }
        }
        public int RemoveEdgeRange(IEnumerable<Edge> edges)
        {
            if (edges is null) throw new ArgumentNullException(nameof(edges));

            int removed = 0;
            foreach (Edge edge in edges)
                if (RemoveEdge(edge)) removed++;
            return removed;
        }

        public void Clear()
        {
            VertexSet.Clear();
            InEdgeSet.Clear();
            OutEdgeSet.Clear();
        }

        public async IAsyncEnumerable<IVertex> GetVertices()
        {
            foreach (Vertex vertex in VertexSet.Values)
            {
                InEdgeSet.TryGetValue(vertex.Identifier, out HashSet<Edge> inEdges);
                OutEdgeSet.TryGetValue(vertex.Identifier, out HashSet<Edge> outEdges);
                yield return await Task.FromResult
                (
                    new InOutVertex
                    (
                        vertex,
                        inEdges ?? new HashSet<Edge>(),
                        outEdges ?? new HashSet<Edge>()
                    )
                );
            }
        }

        public IEnumerable<Identity> Roots
        {
            get
            {
                return VertexSet.Values
                    .Where(vertex => !InEdgeSet.TryGetValue(vertex.Identifier, out HashSet<Edge> edges) || !edges.Any())
                    .Select(vertex => vertex.Identifier);
            }
        }

        public ITask<InOutVertex> GetVertex(Identity id)
        {
            if (!VertexSet.TryGetValue(id, out Vertex vertex))
                return Task.FromResult<InOutVertex>(null).AsITask();

            InEdgeSet.TryGetValue(id, out HashSet<Edge> inEdges);
            OutEdgeSet.TryGetValue(id, out HashSet<Edge> outEdges);
            InOutVertex inoutVertex = new InOutVertex
            (
                vertex,
                inEdges ?? new HashSet<Edge>(),
                outEdges ?? new HashSet<Edge>()
            );
            return Task.FromResult(inoutVertex).AsITask();
        }
        public async IAsyncEnumerable<InOutVertex> GetParentVertices(Identity id)
        {
            InOutVertex vertex = await GetVertex(id);
            foreach (Edge inEdge in vertex.InEdges)
                yield return await GetVertex(inEdge.Source);
        }
        public async IAsyncEnumerable<InOutVertex> GetChildVertices(Identity id)
        {
            InOutVertex vertex = await GetVertex(id);
            foreach (Edge outEdge in vertex.OutEdges)
                yield return await GetVertex(outEdge.Target);
        }
    }
}