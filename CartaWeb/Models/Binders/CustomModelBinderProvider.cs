using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

using CartaCore.Statistics;

namespace CartaWeb.Models.Binders
{
    /// <summary>
    /// Provides custom model binders for complex custom types.
    /// </summary>
    public class CustomModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (typeof(IIntegerDistribution).IsAssignableFrom(context.Metadata.ModelType))
                return new BinderTypeModelBinder(typeof(DistributionModelBinder));

            return null;
        }
    }
}