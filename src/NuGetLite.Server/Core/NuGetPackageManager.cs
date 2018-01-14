using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    /// <summary>
    /// Manage the nuget packages
    /// </summary>
    public class NuGetPackageManager
    {
        private readonly IPersistentStorage persistentStorage;
        private readonly INuGetPackageIndex packageIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetPackageManager"/> class
        /// </summary>
        /// <param name="persistentStorage">The persistent storage instance to be used</param>
        /// <param name="packageIndex">The package index instance to be used</param>
        public NuGetPackageManager(IPersistentStorage persistentStorage, INuGetPackageIndex packageIndex)
        {
            if (persistentStorage == null)
                throw new ArgumentNullException(nameof(persistentStorage));
            if (packageIndex == null)
                throw new ArgumentNullException(nameof(packageIndex));

            this.persistentStorage = persistentStorage;
            this.packageIndex = packageIndex;
        }

        /// <summary>
        /// Search for packages matching the <paramref name="query"/> and <paramref name="includePrerelease"/>
        /// </summary>
        /// <param name="query">The query to be used to search the packages</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="includePrerelease">A value indicating if the pre release packages should be included or not</param>
        /// <returns></returns>
        public async Task<Tuple<int, IEnumerable<NuGetPackageSummary>>> Search(string query, int skip, int take, bool includePrerelease)
        {
            var totalHit = await this.packageIndex.Count(query, includePrerelease);
            var packagesSummary = await this.packageIndex.SearchPackages(query, skip, take, includePrerelease);

            return new Tuple<int, IEnumerable<NuGetPackageSummary>>(totalHit, packagesSummary);
        }

        public async Task PublishPackage(Stream stream)
        {
            using (NuGet.Packaging.PackageArchiveReader packageArchiveReader = new NuGet.Packaging.PackageArchiveReader(stream))
            {
                var packageName = packageArchiveReader.NuspecReader.GetId().ToLower();
                var version = packageArchiveReader.NuspecReader.GetVersion().ToNormalizedString();

                // Write the nupkg file
                await this.persistentStorage.WriteContent($"{packageName}/{version}/{packageName}.{version}.nupkg", stream).ConfigureAwait(false);

                // Write the nuspec file extracted from the nupkg
                var nuspecStream = packageArchiveReader.GetNuspec();
                await this.persistentStorage.WriteContent($"{packageName}/{version}/{packageName}.nuspec", nuspecStream).ConfigureAwait(false);
                
                // Index package
                var packageSummary = await this.packageIndex.IndexPackage(packageArchiveReader.NuspecReader).ConfigureAwait(false);

                // Write the version index.json file
                await this.PublishVersionIndexFile(packageSummary).ConfigureAwait(false);
            }
        }

        private async Task PublishVersionIndexFile(NuGetPackageSummary packageSummary)
        {
            var versionIndexFilePath = $"{packageSummary.Id}/index.json";
            var jsonSerializer = Newtonsoft.Json.JsonSerializer.Create();

            var versions = new
            {
                versions = packageSummary.Versions.Select(v => v.Version)
            };

            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonSerializer.Serialize(jsonWriter, versions);

                jsonWriter.Flush();
                sw.Flush();

                ms.Seek(0, SeekOrigin.Begin);
                await persistentStorage.WriteContent(versionIndexFilePath, ms).ConfigureAwait(false);
            }
        }
    }
}
