using Microsoft.Extensions.Logging;
using NuGet.Packaging.Core;
using NuGetLite.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core.PackageIndex
{
    public abstract class NuGetPackageIndexBase : INuGetPackageIndex
    {
        private readonly ILogger<NuGetPackageIndexBase> logger;
        protected readonly string registrationServiceUrl;
        protected readonly string packageContentServiceUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryNuGetPackageIndex"/> class
        /// </summary>
        /// <param name="serviceIndex">The service index instance to be used</param>
        /// <param name="logger">The logger instance to be used</param>
        protected NuGetPackageIndexBase(ServiceIndex serviceIndex, ILogger<NuGetPackageIndexBase> logger)
        {
            if (serviceIndex == null)
                throw new ArgumentNullException(nameof(serviceIndex));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.logger = logger;
            this.registrationServiceUrl = serviceIndex.Resources.First(r => r.Type == ServiceIndexResourceType.RegistrationBaseUrl).Id;
            this.packageContentServiceUrl = serviceIndex.Resources.First(r => r.Type == ServiceIndexResourceType.PackageBaseAddress).Id;
        }

        /// <summary>
        /// Initializes the package index
        /// </summary>
        /// <returns></returns>
        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Index a package from its metadata
        /// </summary>
        /// <param name="nuspecReader">The nuspec reader instance to be used to read metadata</param>
        /// <returns>The <see cref="RegistrationResult"/> instance which correspond to the package</returns>
        public abstract Task<RegistrationResult> IndexPackage(INuspecCoreReader nuspecReader);

        /// <summary>
        /// Gets count packages match the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to be used to search the packages</param>
        /// <param name="includePrerelease">A value indicating if the pre release packages should be included or not</param>
        /// <returns></returns>
        public abstract Task<int> Count(string query, bool includePrerelease);

        /// <summary>
        /// Return the package summary which match with the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to be used to search the packages</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="includePrerelease">A value indicating if the pre release packages should be included or not</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<NuGetPackageSummary>> SearchPackages(string query, int skip, int take, bool includePrerelease);

        /// <summary>
        /// Handle the logic to index a package from the <paramref name="nuspecReader"/> and store information in the <paramref name="registrationIndex"/> if provided or create a new entry
        /// </summary>
        /// <param name="nuspecReader">The nuspecReader to be used to read package information</param>
        /// <param name="registrationIndex">The <see cref="RegistrationResult"/> instance which should be used to create new version if package id already exists</param>
        /// <returns>The <paramref name="registrationIndex"/> instance of provided or a new one</returns>
        protected virtual async Task<RegistrationResult> IndexPackageCore(INuspecCoreReader nuspecReader, RegistrationResult registrationIndex)
        {
            bool isNewPackageId = false;
            var metadata = nuspecReader.GetMetadata();
            string version = nuspecReader.GetVersion().ToNormalizedString();
            string packageRegistrationBaseUrl = $"{registrationServiceUrl + nuspecReader.GetId()}/index.json";

            if (registrationIndex == null)
            {
                registrationIndex = new RegistrationResult()
                {
                    Id = packageRegistrationBaseUrl
                };

                isNewPackageId = true;
            }

            RegistrationPage registrationPage = registrationIndex.Items.FirstOrDefault();
            if (registrationPage == null)
            {
                registrationPage = new RegistrationPage();
                registrationIndex.Items.Add(registrationPage);
            }

            var versions = registrationPage.Items.FirstOrDefault()?.CatalogEntry.Versions;
            var existingVersions = versions == null ? new List<NuGetPackageVersion>() : new List<NuGetPackageVersion>(versions);

            bool versionExists = existingVersions.Any(v => v.Version == version);
            if (versionExists)
                throw new PackageVersionAlreadyExistsException($"The version {version} already exists for package {nuspecReader.GetId()}");
            existingVersions.Add(new NuGetPackageVersion() { PackageMetadataUrl = registrationServiceUrl + nuspecReader.GetId() + "/" + version, Version = version, Downloads = 0 });

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

            if(isNewPackageId)
                await this.AddRegistrationResult(registrationIndex).ConfigureAwait(false);

            return registrationIndex;
        }

        /// <summary>
        /// Add a new <see cref="RegistrationResult"/> in the index
        /// </summary>
        /// <param name="registrationResult"></param>
        /// <returns></returns>
        protected abstract Task AddRegistrationResult(RegistrationResult registrationResult);


    }
}
