using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server
{
    public static class ServiceIndexMiddleware
    {
        private static string indexContent;

        public static void UseNuGetServiceIndex(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            var resources = new ServiceIndexResource[]
            {
                new ServiceIndexResource(){ Id = "http://localhost:50175/api/v2", Type = "PackagePublish/2.0.0"}
            };
            var serviceIndex = new ServiceIndex()
            {
                Version = "3.0.0-beta.1",
                Resources = resources
            };

            indexContent = JsonConvert.SerializeObject(serviceIndex);
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

        public class ServiceIndex
        {
            /// <summary>
            /// Gets or sets the service index version
            /// </summary>
            public string Version
            { get; set; }

            public IEnumerable<ServiceIndexResource> Resources
            { get; set; }
        }

        public class ServiceIndexResource
        {
            [JsonProperty(propertyName: "@id")]
            public string Id
            { get; set; }

            [JsonProperty(propertyName: "@type")]
            public string Type
            { get; set; }

            public string Comment
            { get; set; }
        }
    }
}
