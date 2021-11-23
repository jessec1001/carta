namespace CartaCore.Data
{
    /// <summary>
    /// An interface for providing a specific base type of an instance.
    /// </summary>
    /// <typeparam name="T">The type of provider.</typeparam>
    public interface IProvider<T>
    {
        /// <summary>
        /// Tries to provide this instance as the specified interface
        /// </summary>
        /// <param name="func">The instance as the specified interface.</param>
        /// <typeparam name="U">The type of interface to provide.</typeparam>
        /// <returns><c>true</c> if the provision was succesful; otherwise <c>false</c>.</returns>
        bool TryProvide<U>(out U func) where U : T
        {
            if (this is U fn)
            {
                func = fn;
                return true;
            }
            else
            {
                func = default;
                return false;
            }
        }
        /// <summary>
        /// Determines whether this instance can provide the specified interface.
        /// </summary>
        /// <typeparam name="U">The type of interface to provide.</typeparam>
        /// <returns><c>true</c> if the provision can be made; otherwise <c>false</c>.</returns>
        bool CanProvide<U>() where U : T
        {
            return TryProvide(out U _);
        }
        /// <summary>
        /// Provides this instance as the specified interface.
        /// </summary>
        /// <typeparam name="U">The type of interface to provide.</typeparam>
        /// <returns>The instance as the specified interface.</returns>
        U Provide<U>() where U : T
        {
            TryProvide(out U func);
            return func;
        }
    }
}