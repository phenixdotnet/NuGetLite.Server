using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGetLite.Server.Models;
using System;
using Microsoft.Extensions.DependencyInjection;

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

            var serviceIndex = app.ApplicationServices.GetService<ServiceIndex>();

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
