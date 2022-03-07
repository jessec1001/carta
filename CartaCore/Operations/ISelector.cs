using System;
using System.Threading.Tasks;

namespace CartaCore.Operations
{
    /// <summary>
    /// Represents the base functionality of selecting a source object into a target object.
    /// </summary>
    /// <typeparam name="TSource">The type of source object.</typeparam>
    /// <typeparam name="TTarget">The type of target object.</typeparam>
    public interface ISelector<TSource, TTarget>
    {
        /// <summary>
        /// The type of parameters that should be used. 
        /// </summary>
        Type ParameterType { get; }

        /// <summary>
        /// Performs a selection on a source producing a target.
        /// </summary>
        /// <param name="source">The source of the selection.</param>
        /// <param name="parameters">The parameters to use for selection.</param>
        /// <returns>The target of the selection.</returns>
        Task<TTarget> Select(TSource source, object parameters);
    }
}