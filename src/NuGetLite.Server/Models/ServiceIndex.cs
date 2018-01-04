using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Models
{
    /// <summary>
    /// Represent the ServiceIndex json response
    /// </summary>
    public class ServiceIndex
    {
        /// <summary>
        /// Gets or sets the service index version
        /// </summary>
        public string Version
        { get; set; }

        /// <summary>
        /// Gets or sets the resources of the service index
        /// </summary>
        public IEnumerable<ServiceIndexResource> Resources
        { get; set; }
    }

    /// <summary>
    /// Represent the service index resource json response
    /// </summary>
    public class ServiceIndexResource
    {
        /// <summary>
        /// Gets or sets the resource id
        /// </summary>
        [JsonProperty(propertyName: "@id")]
        public string Id
        { get; set; }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        [JsonProperty(propertyName: "@type")]
        public string Type
        { get; set; }

        /// <summary>
        /// Gets or sets the optional comment
        /// </summary>
        public string Comment
        { get; set; }
    }

    /// <summary>
    /// Define the service index resource type available
    /// </summary>
    public static class ServiceIndexResourceType
    {
        /// <summary>
        /// The search query service type
        /// </summary>
        public const string SearchQuery = "SearchQueryService";

        /// <summary>
        /// The registration type
        /// </summary>
        public const string Registration = "RegistrationsBaseUrl";

        /// <summary>
        /// 
        /// </summary>
        public const string PackageBaseAddress = "PackageBaseAddress/3.0.0";

        /// <summary>
        /// The publish type
        /// </summary>
        public const string Publish = "PackagePublish/2.0.0";
    }
}
