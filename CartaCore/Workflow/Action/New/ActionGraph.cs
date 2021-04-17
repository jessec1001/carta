using System;
using System.Collections.Generic;
using System.Linq;

using MorseCode.ITask;

using CartaCore.Data;

namespace CartaCore.Workflow.Action
{
    public class ActionGraph : WrapperGraph,
        IEntireGraph,
        IDynamicGraph<IVertex>
    {
        public IGraph Graph { get; set; }

        protected override IGraph WrappedGraph => Graph;

        protected virtual bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            if (type.IsAssignableTo(typeof(IDynamicGraph<IVertex>))) return Graph.CanProvide<IDynamicGraph<IVertex>>();
            return true;
        }

        public virtual Edge TransformEdge(Edge edge) => edge;
        public virtual Vertex TransformVertex(Vertex vertex) => vertex;

        public override bool TryProvide<U>(out U func)
        {
            bool shouldProvide = ShouldProvide(typeof(U));
            if (!shouldProvide)
            {
                func = default(U);
                return false;
            }

            bool success = base.TryProvide<U>(out U fn);
            func = fn;
            return success;
        }

        public ITask<IVertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
                return dynamic.GetVertex(id);
            return null;
        }
        public IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
                return entire.GetVertices();
            else return null;
        }
    }
}