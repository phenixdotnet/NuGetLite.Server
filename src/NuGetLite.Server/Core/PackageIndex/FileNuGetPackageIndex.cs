﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using NuGet.Packaging.Core;
using NuGetLite.Server.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core.PackageIndex
{
    public class FileNuGetPackageIndex : NuGetPackageIndexBase
    {
        private readonly ILogger<FileNuGetPackageIndex> logger;

        private readonly List<RegistrationResult> packages;
        private readonly Dictionary<string, List<NuGetPackageVersion>> versions;
        private readonly IPersistentStorage packageListStorage;
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileNuGetPackageIndex"/> class
        /// </summary>
        /// <param name="serviceIndex">The service index instance to be used to generate package urls</param>
        /// <param name="packageListStorage">The <see cref="IPersistentStorage"/> instance to be used to persist the package list</param>
        /// <param name="logger">The logger instance to be used</param>
        /// <param name="telemetryClient">The telemetry client instance to be used</param>
        public FileNuGetPackageIndex(ServiceIndex serviceIndex, IPersistentStorage packageListStorage, ILogger<FileNuGetPackageIndex> logger, TelemetryClient telemetryClient)
            : base(serviceIndex, logger)
        {
            if (packageListStorage == null)
                throw new ArgumentNullException(nameof(packageListStorage));

            this.packages = new List<RegistrationResult>();
            this.versions = new Dictionary<string, List<NuGetPackageVersion>>();
            this.packageListStorage = packageListStorage;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Initializes the package index
        /// </summary>
        /// <returns></returns>
        public override async Task Initialize()
        {
            var savedPackageList = await this.packageListStorage.LoadContent<IEnumerable<RegistrationResult>>("packages.json").ConfigureAwait(false);
            if (savedPackageList != null)
                this.packages.AddRange(savedPackageList);
        }

        /// <summary>
        /// Index a package from its metadata
        /// </summary>
        /// <param name="nuspecReader">The nuspec reader instance to be used to read metadata</param>
        /// <returns>The <see cref="RegistrationResult"/> instance which correspond to the package</returns>
        public override async Task<RegistrationResult> IndexPackage(INuspecCoreReader nuspecReader)
        {
            if (nuspecReader == null)
                throw new ArgumentNullException(nameof(nuspecReader));

            var indexingStopwatch = Stopwatch.StartNew();

            string version = nuspecReader.GetVersion().ToNormalizedString();
            string packageRegistrationBaseUrl = $"{registrationServiceUrl + nuspecReader.GetId()}/index.json";

            logger.LogDebug(LoggingEvents.PackageIndexIndexingPackage, "Indexing package {packageId}, version {version}", nuspecReader.GetId(), version);

            RegistrationResult registrationIndex = this.packages.FirstOrDefault(p => p.Id == packageRegistrationBaseUrl);
            registrationIndex = await this.IndexPackageCore(nuspecReader, registrationIndex).ConfigureAwait(false);

            await this.SavePackageIndex().ConfigureAwait(false);

            indexingStopwatch.Stop();
            logger.LogInformation(LoggingEvents.PackageIndexPackageIndexed, "Package {packageId}, version {version} indexed in {elapsed}", nuspecReader.GetId(), version, indexingStopwatch.Elapsed);

            var metricEvent = new EventTelemetry("PackageIndexed");
            metricEvent.Metrics.Add("packageIndexed", indexingStopwatch.ElapsedMilliseconds);
            telemetryClient.TrackEvent(metricEvent);

            return registrationIndex;
        }

        /// <summary>
        /// Gets count packages match the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to be used to search the packages</param>
        /// <param name="includePrerelease">A value indicating if the pre release packages should be included or not</param>
        /// <returns></returns>
        public override Task<int> Count(string query, bool includePrerelease)
        {
            int count = (from r in this.packages
                         from p in r.Items
                         from l in p.Items
                         where PackageMatchQuery(query, includePrerelease, l.CatalogEntry)
                         select l.CatalogEntry).Count();

            return Task.FromResult(count);
        }

        /// <summary>
        /// Return the package summary which match with the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to be used to search the packages</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="includePrerelease">A value indicating if the pre release packages should be included or not</param>
        /// <returns></returns>
        public override Task<IEnumerable<NuGetPackageSummary>> SearchPackages(string query, int skip, int take, bool includePrerelease)
        {
            var results = (from r in this.packages
                           from p in r.Items
                           from l in p.Items
                           where PackageMatchQuery(query, includePrerelease, l.CatalogEntry)
                           select l.CatalogEntry).Skip(skip).Take(take);

            return Task.FromResult(results);
        }

        /// <summary>
        /// Gets all versions for the <paramref name="packageId"/>
        /// </summary>
        /// <param name="packageId">The package id to be used</param>
        /// <returns></returns>
        public override Task<IEnumerable<string>> GetAllVersions(string packageId)
        {
            var versions = from v in this.versions[packageId]
                           select v.Version;

            return Task.FromResult(versions);
        }

        protected override async Task IncrementDownloadCounterCore(string packageName, string version)
        {
            string key = packageName + version;
            if (this.versions.ContainsKey(key))
            {
                var packageVersion = (from v in versions[packageName]
                                      where v.Version == version
                                      select v).First();


                ++packageVersion.Downloads;
                await this.SavePackageIndex().ConfigureAwait(false);
            }
        }

        protected override Task AddRegistrationResult(RegistrationResult registrationResult)
        {
            this.packages.Add(registrationResult);

            return Task.CompletedTask;
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

        private async Task SavePackageIndex()
        {
            await this.packageListStorage.WriteContent("packages.json", this.packages).ConfigureAwait(false);
        }

        protected override bool IsVersionAlreadyExisting(string packageId, string version)
        {
            return this.versions.ContainsKey(packageId + version);
        }

        protected override Task AddNewVersion(string packageId, NuGetPackageVersion nuGetPackageVersion)
        {
            if (!this.versions.ContainsKey(packageId))
                this.versions.Add(packageId, new List<NuGetPackageVersion>());
            this.versions[packageId].Add(nuGetPackageVersion);
            return Task.CompletedTask;
        }
    }
}
