using System.Collections.Generic;

namespace CartaCore.Data
{
    public abstract class WrapperGraph : IGraph
    {
        protected abstract IGraph WrappedGraph { get; }

        public bool IsDirected() => WrappedGraph is null ? false : WrappedGraph.IsDirected();
        public bool IsDynamic() => WrappedGraph is null ? false : WrappedGraph.IsDynamic();
        public bool IsFinite() => WrappedGraph is null ? false : WrappedGraph.IsFinite();

        public virtual bool TryProvide<U>(out U func) where U : IGraph
        {
            if (this is U fn)
            {
                func = fn;
                return true;
            }
            else if (WrappedGraph.TryProvide<U>(out U wrappedFn))
            {
                func = wrappedFn;
                return true;
            }
            else
            {
                func = default(U);
                return false;
            }
        }
    }
}