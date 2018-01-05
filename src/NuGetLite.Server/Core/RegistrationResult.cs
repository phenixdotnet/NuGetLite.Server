using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class RegistrationResult
    {
        /// <summary>
        /// Gets or sets the number of registration pages in the index
        /// </summary>
        public int Count
        { get; set; }

        /// <summary>
        /// Gets or sets the registration pages
        /// </summary>
        public IEnumerable<RegistrationPage> Items
        { get; set; }
    }

    public class RegistrationPage
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id
        { get; set; }

        /// <summary>
        /// Gets or sets the number of registration leaves in the page
        /// </summary>
        public int Count
        { get; set; }

        /// <summary>
        /// Gets or sets the lower version
        /// </summary>
        public string Lower
        { get; set; }

        /// <summary>
        /// Gets or sets the upper version
        /// </summary>
        public string Upper
        { get; set; }

        public IEnumerable<RegistrationLeaf> Items
        { get; set; }
    }

    public class RegistrationLeaf
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id
        { get; set; }

        public string PackageContent
        { get; set; }

        public NuGetPackageSummary CatalogEntry
        { get; set; }
    }
}
