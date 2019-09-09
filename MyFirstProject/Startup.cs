using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using MyFirstProject.Common;
using MyFiratProject.Dal;

namespace MyFirstProject
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore().AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VV";
                    options.SubstituteApiVersionInUrl = true;
                });

            services.AddMvc().AddControllersAsServices()
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddApiVersioning(o => o.ReportApiVersions = true);
            services.AddSwaggerGen(
                c =>
                {
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

                    foreach (var description in provider.ApiVersionDescriptions)
                        c.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));

                    // add a custom operation filter which sets default values
                    c.DocumentFilter<SecurityRequirementsDocumentFilter>();
                });
        }

        private static Info CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new Info
            {
                Title = "",
                Version = description.ApiVersion.ToString(),
                Description = "."
            };

            if (description.IsDeprecated) info.Description += " This API version has been deprecated.";

            return info;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            ConfigureSwagger(app, provider);
            app.UseHttpsRedirection();
            app.UseMvc();
            
        }

        private static void ConfigureSwagger(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                    c.SwaggerEndpoint(
                        // relative path without leading "/swagger/" beacuse it will be work identical with different locations
                        // like "http://localhost:5000/swagger/..." or "http://localhost/sp/swagger/..."
                        $"{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
            });
        }

        public void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            var settings = new AppSettings();
            Configuration.Bind("AppSettings", settings);

            containerBuilder.Register(x => settings).As<AppSettings>().SingleInstance();

            containerBuilder.RegisterType<Repository>().AsSelf();
        }
    }
}
