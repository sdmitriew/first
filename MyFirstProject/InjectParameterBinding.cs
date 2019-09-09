using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace MyFirstProject
{
    public class InjectParameterBinding : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var service = bindingContext.HttpContext.RequestServices.GetService(bindingContext.ModelType);
            bindingContext.Model = service;
            bindingContext.Result = ModelBindingResult.Success(service);
            return Task.CompletedTask;
        }
    }

    public class ProtectedIdModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (!context.Metadata.IsComplexType) return null;
            if (!context.BindingInfo.BindingSource.IsFromRequest)
                return new BinderTypeModelBinder(typeof(InjectParameterBinding));
            return null;
        }
    }
}
