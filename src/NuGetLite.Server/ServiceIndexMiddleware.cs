using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGetLite.Server.Models;
using System;
using System.Linq;

namespace NuGetLite.Server
{
    /// <summary>
    /// Define the Service Index middleware which provide "static" index.json file with services information
    /// </summary>
    public static class ServiceIndexMiddleware
    {
        private static string indexContent;

        /// <summary>
        /// Register the service index middleware at path "/v3/index.json"
        /// </summary>
        /// <param name="app"></param>
        public static void UseNuGetServiceIndex(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            //var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
            string baseUrl = "http://localhost:55981";

            var resources = new ServiceIndexResource[]
            {
                new ServiceIndexResource(){ Id = baseUrl + "/query", Type = ServiceIndexResourceType.SearchQuery },
                new ServiceIndexResource() { Id = baseUrl + "/registration/", Type = ServiceIndexResourceType.Registration },
                new ServiceIndexResource() { Id = baseUrl + "/v3-flatcontainer/", Type = ServiceIndexResourceType.PackageBaseAddress, Comment = "Base URL of where NuGet packages are stored, in the format https://api.nuget.org/v3-flatcontainer/{id-lower}/{version-lower}/{id-lower}.{version-lower}.nupkg"},
                new ServiceIndexResource(){ Id = baseUrl + "/api/v2", Type = ServiceIndexResourceType.Publish}
            };
            var serviceIndex = new ServiceIndex()
            {
                Version = "3.0.0-beta.1",
                Resources = resources
            };

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.None;
            jsonSerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            indexContent = JsonConvert.SerializeObject(serviceIndex, jsonSerializerSettings);
            app.Map("/v3/index.json", HandleIndexJsonPath);
        }

        private static void HandleIndexJsonPath(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(indexContent);
            });
        }
    }
}
