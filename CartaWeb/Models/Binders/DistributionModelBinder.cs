using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using CartaCore.Statistics;

namespace CartaWeb.Models.Binders
{
    /// <summary>
    /// Binds values to statistical distribution models.
    /// </summary>
    public class DistributionModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Set the model to null and to a value when found.
            IIntegerDistribution distribution = null;

            // Check if the binding context is null before continuing.
            if (bindingContext is null) throw new ArgumentNullException(nameof(bindingContext));

            // Get the value from value provider.
            string modelName = bindingContext.ModelName;
            ValueProviderResult value = bindingContext.ValueProvider.GetValue(modelName);
            if (value != ValueProviderResult.None)
            {
                string source = value.Values.ToString();
                IIntegerDistribution.TryParse(source, out distribution);
            }

            // Return a successful result.
            if (!(distribution is null)) bindingContext.Result = ModelBindingResult.Success(distribution);
            return Task.CompletedTask;
        }
    }
}