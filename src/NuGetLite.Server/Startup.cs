using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NuGetLite.Server.Core;
using NuGetLite.Server.Models;

namespace NuGetLite.Server
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
            services.AddMvc();

            this.AddDependencies(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseNuGetServiceIndex();
        }

        private void AddDependencies(IServiceCollection services)
        {
            services.AddSingleton<ServiceIndex>(CreateServiceIndex());
            services.AddSingleton<IPersistentStorage, FilePersistentStorage>();
            services.AddSingleton<INuGetPackageIndex, InMemoryNuGetPackageIndex>();
            services.AddSingleton<NuGetPackageManager>();
        }

        private ServiceIndex CreateServiceIndex()
        {
            string baseUrl = "http://localhost:49869";

            var resources = new ServiceIndexResource[]
            {
                new ServiceIndexResource(){ Id = baseUrl + "/query", Type = ServiceIndexResourceType.SearchQueryService },
                new ServiceIndexResource() { Id = baseUrl + "/registration/", Type = ServiceIndexResourceType.RegistrationBaseUrl },
                new ServiceIndexResource() { Id = baseUrl + "/v3-flatcontainer/", Type = ServiceIndexResourceType.PackageBaseAddress, Comment = "Base URL of where NuGet packages are stored, in the format https://api.nuget.org/v3-flatcontainer/{id-lower}/{version-lower}/{id-lower}.{version-lower}.nupkg"},
                new ServiceIndexResource(){ Id = baseUrl + "/api/v2", Type = ServiceIndexResourceType.PackagePublish}
            };
            var serviceIndex = new ServiceIndex()
            {
                Version = "3.0.0-beta.1",
                Resources = resources
            };

            return serviceIndex;
        }
    }
}
