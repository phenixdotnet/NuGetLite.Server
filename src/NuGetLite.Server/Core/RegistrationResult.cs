using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class RegistrationResult
    {
        public RegistrationResult()
        {
            this.Items = new HashSet<RegistrationPage>();
        }

        /// <summary>
        /// Gets or sets the base url
        /// </summary>
        [JsonProperty(PropertyName = "@id")]
        public string Id
        { get; set; }

        /// <summary>
        /// Gets or sets the number of registration pages in the index
        /// </summary>
        public int Count
        { get { return this.Items.Count; } }

        /// <summary>
        /// Gets or sets the registration pages
        /// </summary>
        public ICollection<RegistrationPage> Items
        { get; set; }
    }

    public class RegistrationPage
    {
        public RegistrationPage()
        {
            this.Items = new HashSet<RegistrationLeaf>();
        }

        [JsonProperty(PropertyName = "@id")]
        public string Id
        { get; set; }

        /// <summary>
        /// Gets or sets the number of registration leaves in the page
        /// </summary>
        public int Count
        { get { return this.Items.Count; } }

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

        /// <summary>
        /// Gets or sets the items
        /// </summary>
        public ICollection<RegistrationLeaf> Items
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
