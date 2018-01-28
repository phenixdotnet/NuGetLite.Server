using Newtonsoft.Json;
using NuGet.Packaging.Core;
using NuGetLite.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class InMemoryNuGetPackageIndex : INuGetPackageIndex
    {
        private readonly HashSet<RegistrationResult> packages;
        private readonly ServiceIndex serviceIndex;
        private readonly string registrationServiceUrl;
        private readonly string packageContentServiceUrl;
        private readonly IPersistentStorage persistentStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryNuGetPackageIndex"/> class
        /// </summary>
        /// <param name="serviceIndex">The service index instance to be used</param>
        public InMemoryNuGetPackageIndex(ServiceIndex serviceIndex)
        {
            if (serviceIndex == null)
                throw new ArgumentNullException(nameof(serviceIndex));

            this.packages = new HashSet<RegistrationResult>();
            this.serviceIndex = serviceIndex;
            this.registrationServiceUrl = serviceIndex.Resources.First(r => r.Type == ServiceIndexResourceType.RegistrationBaseUrl).Id;
            this.packageContentServiceUrl = serviceIndex.Resources.First(r => r.Type == ServiceIndexResourceType.PackageBaseAddress).Id;
        }

        public Task<RegistrationResult> IndexPackage(INuspecCoreReader nuspecReader)
        {
            if (nuspecReader == null)
                throw new ArgumentNullException(nameof(nuspecReader));

            var metadata = nuspecReader.GetMetadata();

            string version = nuspecReader.GetVersion().ToNormalizedString();
            string packageRegistrationBaseUrl = $"{registrationServiceUrl + nuspecReader.GetId()}/index.json";

            RegistrationResult registrationIndex = this.packages.FirstOrDefault(p => p.Id == packageRegistrationBaseUrl);

            if (registrationIndex == null)
            {
                registrationIndex = new RegistrationResult()
                {
                    Id = packageRegistrationBaseUrl
                };

                this.packages.Add(registrationIndex);
            }

            RegistrationPage registrationPage = registrationIndex.Items.FirstOrDefault();
            if (registrationPage == null)
            {
                registrationPage = new RegistrationPage();
                registrationIndex.Items.Add(registrationPage);
            }

            var versions = registrationPage.Items.FirstOrDefault()?.CatalogEntry.Versions;
            var existingVersions = versions == null ? new List<NuGetPackageVersion>() : new List<NuGetPackageVersion>(versions);
            existingVersions.Add(new NuGetPackageVersion() { PackageMetadataUrl = registrationServiceUrl + nuspecReader.GetId() + "/" + version, Version = nuspecReader.GetVersion().ToFullString(), Downloads = 0 });

            var packageSummary = new NuGetPackageSummary()
            {
                PackageMetadataUrl = registrationServiceUrl + nuspecReader.GetId() + "/index.json",
                Id = nuspecReader.GetId(),
                Version = nuspecReader.GetVersion().ToFullString(),
                Versions = existingVersions
            };

            foreach (var m in metadata)
            {
                if ("title".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.Title = m.Value;
                else if ("description".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.Description = m.Value;
                else if ("authors".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(m.Value))
                    packageSummary.Authors = m.Value.Split(",");
                else if ("owners".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(m.Value))
                    packageSummary.Owners = m.Value.Split(",");
                else if ("requireLicenseAcceptance".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.RequireLicenseAcceptance = bool.Parse(m.Value);
                else if ("licenseUrl".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.LicenseUrl = m.Value;
                else if ("projectUrl".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.ProjectUrl = m.Value;
                else if ("iconUrl".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.IconUrl = m.Value;
                else if ("copyright".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.Copyright = m.Value;
                else if ("tags".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.Tags = m.Value;
            }

            RegistrationLeaf registrationLeaf = new RegistrationLeaf();
            registrationLeaf.CatalogEntry = packageSummary;
            registrationLeaf.PackageContent = $"{this.packageContentServiceUrl}{nuspecReader.GetId()}/{version}/{nuspecReader.GetId()}.{version}.nupkg";

            string lowerVersion = packageSummary.Versions.First().Version;
            string upperVersion = packageSummary.Versions.Last().Version;

            registrationPage.Id = $"{registrationServiceUrl + nuspecReader.GetId()}/index.json/#page/{lowerVersion}/{upperVersion}";
            registrationPage.Items.Add(registrationLeaf);
            registrationPage.Lower = lowerVersion;
            registrationPage.Upper = upperVersion;


            return Task.FromResult(registrationIndex);
        }

        public Task<int> Count(string query, bool includePrerelease)
        {
            int count = (from r in this.packages
                         from p in r.Items
                         from l in p.Items
                         where PackageMatchQuery(query, includePrerelease, l.CatalogEntry)
                         select l.CatalogEntry).Count();

            return Task.FromResult(count);
        }

        public Task<IEnumerable<NuGetPackageSummary>> SearchPackages(string query, int skip, int take, bool includePrerelease)
        {
            var results = (from r in this.packages
                           from p in r.Items
                           from l in p.Items
                           where PackageMatchQuery(query, includePrerelease, l.CatalogEntry)
                           select l.CatalogEntry).Skip(skip).Take(take);

            return Task.FromResult(results);
        }

        private bool PackageMatchQuery(string query, bool includePrerelease, NuGetPackageSummary package)
        {
            if (!includePrerelease && package.IsPrerelease)
                return false;

            if (string.IsNullOrEmpty(query))
                return true;

            return package.Id.Contains(query.ToLower())
                || (!string.IsNullOrEmpty(package.Title) && package.Title.Contains(query))
                || (!string.IsNullOrEmpty(package.Description) && package.Description.Contains(query))
                || (!string.IsNullOrEmpty(package.Tags) && package.Tags.Contains(query));
        }
    }
}
