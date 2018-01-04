using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class NuGetPackageSummary
    {
        public string Id
        { get; set; }

        public string Version
        { get; set; }

        public IEnumerable<NuGetPackageVersion> Versions
        { get; set; }

        public string Description
        { get; set; }

        public IEnumerable<string> Authors
        { get; set; }
    }

    public class NuGetPackageVersion
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id
        { get; set; }

        public string Version
        { get; set; }

        public int Downloads
        { get; set; }
    }
}
