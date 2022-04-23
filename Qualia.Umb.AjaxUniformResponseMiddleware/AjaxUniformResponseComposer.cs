using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Qualia.Umb.AjaxUniformResponseMiddleware
{
    public class AjaxUniformResponseComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter(
                 name: nameof(ResponseWrapperMiddleware),
                 prePipeline: applicationBuilder => { },
                 postPipeline: applicationBuilder => { applicationBuilder.UseMiddleware<ResponseWrapperMiddleware>(); },
                 endpointCallback: applicationBuilder => { }));
            });
        }
    }
}
