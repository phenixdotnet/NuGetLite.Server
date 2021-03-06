﻿using Microsoft.Extensions.Logging;
using NuGet.Packaging.Core;
using NuGetLite.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
        /// Increment the package download counter for the package specified by the <paramref name="packageFilePath"/>
        /// </summary>
        /// <param name="packageFilePath">The package file path which should be used to find the package</param>
        /// <returns></returns>
        public async Task IncrementDownloadCounter(string packageFilePath)
        {
            if (string.IsNullOrEmpty(packageFilePath))
                throw new ArgumentNullException(nameof(packageFilePath));

            // BASEPATH\TestPackage\1.0.0\TestPackage.nuspec
            var packagePathParts = packageFilePath.Split(Path.DirectorySeparatorChar).TakeLast(3).ToArray(); // packagePathParts contains PackageName at index 0, version at index 1, package file name at index 2
            await IncrementDownloadCounterCore(packagePathParts[0], packagePathParts[1]).ConfigureAwait(false);
        }

        /// <summary>
        /// Increment the package download counter for the <paramref name="packageName"/> and version <paramref name="version"/>
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        protected abstract Task IncrementDownloadCounterCore(string packageName, string version);

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

        public abstract Task<IEnumerable<string>> GetAllVersions(string packageId);

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

            bool versionExists = this.IsVersionAlreadyExisting(nuspecReader.GetId(), version);
            if (versionExists)
                throw new PackageVersionAlreadyExistsException($"The version {version} already exists for package {nuspecReader.GetId()}");
            
            var packageSummary = new NuGetPackageSummary()
            {
                PackageMetadataUrl = registrationServiceUrl + nuspecReader.GetId() + "/index.json",
                Id = nuspecReader.GetId(),
                Version = nuspecReader.GetVersion().ToFullString()
            };

            await this.AddNewVersion(packageSummary.Id, new NuGetPackageVersion() { PackageMetadataUrl = registrationServiceUrl + nuspecReader.GetId() + "/" + version, Version = version, Downloads = 0 });

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

            var versions = await this.GetAllVersions(packageSummary.Id).ConfigureAwait(false);
            string lowerVersion = versions.First();
            string upperVersion = versions.Last();
            
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

        /// <summary>
        /// Gets a value indicating if the <paramref name="version"/> is already in the package index
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="version">The version number which should be checked</param>
        /// <returns></returns>
        protected abstract bool IsVersionAlreadyExisting(string packageId, string version);

        /// <summary>
        /// Add a new version of a package
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="nuGetPackageVersion"></param>
        /// <returns></returns>
        protected abstract Task AddNewVersion(string packageId, NuGetPackageVersion nuGetPackageVersion);
    }
}
