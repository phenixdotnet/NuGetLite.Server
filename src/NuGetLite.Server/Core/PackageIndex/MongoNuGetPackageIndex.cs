using Microsoft.Extensions.Logging;
using NuGet.Packaging.Core;
using NuGetLite.Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core.PackageIndex
{
    public class MongoNuGetPackageIndex : NuGetPackageIndexBase
    {
        private readonly ILogger<MongoNuGetPackageIndex> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoNuGetPackageIndex"/> class
        /// </summary>
        /// <param name="serviceIndex">The service index instance to be used</param>
        /// <param name="logger">The logger instance to be used</param>
        public MongoNuGetPackageIndex(ServiceIndex serviceIndex, ILogger<MongoNuGetPackageIndex> logger)
            : base(serviceIndex, logger)
        {
            if (serviceIndex == null)
                throw new ArgumentNullException(nameof(serviceIndex));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.logger = logger;
        }

        public override Task<int> Count(string query, bool includePrerelease)
        {
            throw new NotImplementedException();
        }

        public override Task<RegistrationResult> IndexPackage(INuspecCoreReader nuspecReader)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<NuGetPackageSummary>> SearchPackages(string query, int skip, int take, bool includePrerelease)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all versions for the <paramref name="packageId"/>
        /// </summary>
        /// <param name="packageId">The package id to be used</param>
        /// <returns></returns>
        public override Task<IEnumerable<string>> GetAllVersions(string packageId)
        {
            throw new NotImplementedException();
        }

        protected override Task AddNewVersion(string pacakgeId, NuGetPackageVersion nuGetPackageVersion)
        {
            throw new NotImplementedException();
        }

        protected override Task AddRegistrationResult(RegistrationResult registrationResult)
        {
            throw new NotImplementedException();
        }

        protected override Task IncrementDownloadCounterCore(string packageName, string version)
        {
            throw new NotImplementedException();
        }

        protected override bool IsVersionAlreadyExisting(string packageName, string version)
        {
            throw new NotImplementedException();
        }
    }
}
