namespace CartaCore.Common
{
    public interface IProvider<T>
    {
        bool TryProvide<U>(out U func) where U : T
        {
            if (this is U fn)
            {
                func = fn;
                return true;
            }
            else
            {
                func = default(U);
                return false;
            }
        }
        bool CanProvide<U>() where U : T
        {
            return this.TryProvide<U>(out U _);
        }
        U Provide<U>() where U : T
        {
            this.TryProvide<U>(out U func);
            return func;
        }
    }
}