﻿using Newtonsoft.Json;
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
        private readonly IPersistentStorage persistentStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryNuGetPackageIndex"/> class
        /// </summary>
        /// <param name="serviceIndex">The service index instance to be used</param>
        /// <param name="persistentStorage">The persistent storage instance to be used</param>
        public InMemoryNuGetPackageIndex(ServiceIndex serviceIndex, IPersistentStorage persistentStorage)
        {
            if (serviceIndex == null)
                throw new ArgumentNullException(nameof(serviceIndex));
            if (persistentStorage == null)
                throw new ArgumentNullException(nameof(persistentStorage));

            this.persistentStorage = persistentStorage;
            this.packages = new HashSet<RegistrationResult>();
            this.serviceIndex = serviceIndex;
            this.registrationServiceUrl = serviceIndex.Resources.First(r => r.Type == ServiceIndexResourceType.RegistrationBaseUrl).Id;
        }

        public Task<NuGetPackageSummary> IndexPackage(INuspecCoreReader nuspecReader)
        {
            if (nuspecReader == null)
                throw new ArgumentNullException(nameof(nuspecReader));

            var metadata = nuspecReader.GetMetadata();

            string version = nuspecReader.GetVersion().ToNormalizedString();
            var packageSummary = new NuGetPackageSummary()
            {
                PackageMetadataUrl = registrationServiceUrl + nuspecReader.GetId() + "/index.json",
                Id = nuspecReader.GetId(),
                Version = nuspecReader.GetVersion().ToFullString(),
                Versions = new NuGetPackageVersion[]
                   {
                       new NuGetPackageVersion(){ PackageMetadataUrl = registrationServiceUrl + nuspecReader.GetId() + "/" + version, Version = nuspecReader.GetVersion().ToFullString(), Downloads = 0}
                   }
            };

            foreach (var m in metadata)
            {
                if ("authors".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(m.Value))
                    packageSummary.Authors = m.Value.Split(",");

                if ("tags".Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
                    packageSummary.Tags = m.Value;
            }

            var registrationPage = new RegistrationPage()
            {
                Id = $"{registrationServiceUrl + nuspecReader.GetId()}#page/{version}/{version}",
                Count = registrationLeave.Count,
            };

            var registrationIndex = new RegistrationResult()
            {
                Count = 1,
                Items = new RegistrationPage[1]
                {
                    registrationPage
                }
            };

            this.packages.Add(registrationIndex);


            return Task.FromResult(packageSummary);
        }

        public Task<int> Count(string query, bool includePrerelease)
        {
            int count = (from p in this.packages where PackageMatchQuery(query, includePrerelease, p) select p).Count();

            return Task.FromResult(count);
        }

        public Task<IEnumerable<NuGetPackageSummary>> SearchPackages(string query, int skip, int take, bool includePrerelease)
        {
            var results = (from p in this.packages where PackageMatchQuery(query, includePrerelease, p) select p).Skip(skip).Take(take);

            return Task.FromResult(results);
        }

        private bool PackageMatchQuery(string query, bool includePrerelease, NuGetPackageSummary package)
        {
            if (!includePrerelease && package.IsPrerelease)
                return false;

            if (string.IsNullOrEmpty(query))
                return true;

            return package.Id.Contains(query.ToLower())
                || (!string.IsNullOrEmpty(package.Description) && package.Description.Contains(query))
                || (!string.IsNullOrEmpty(package.Tags) && package.Tags.Contains(query));
        }
    }
}
