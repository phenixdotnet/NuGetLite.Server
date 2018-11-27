using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class NuGetPackageSummary
    {

        public NuGetPackageSummary()
        {
            this.Type = "Package";
        }

        public string Id
        { get; set; }

        public string Title
        { get; set; }

        public string Description
        { get; set; }

        public string LicenseUrl
        { get; set; }

        public string ProjectUrl
        { get; set; }

        public bool RequireLicenseAcceptance
        { get; set; }

        public string IconUrl
        { get; set; }

        public string Copyright
        { get; set; }

        public IEnumerable<string> Owners
        { get; set; }

        public IEnumerable<string> Authors
        { get; set; }

        public string Version
        { get; set; }

        public bool IsPrerelease
        { get; set; }

        public string Tags
        { get; set; }

        [JsonProperty(PropertyName = "@id")]
        public string PackageMetadataUrl
        { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public string Type
        { get; set; }
    }

    public class NuGetPackageVersion
    {
        [JsonProperty(PropertyName = "@id")]
        public string PackageMetadataUrl
        { get; set; }

        public string Version
        { get; set; }

        public int Downloads
        { get; set; }
    }
}
