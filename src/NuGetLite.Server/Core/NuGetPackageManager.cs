using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class NuGetPackageManager
    {
        private readonly IPersistentStorage persistentStorage;

        public NuGetPackageManager(IPersistentStorage persistentStorage)
        {
            if (persistentStorage == null)
                throw new ArgumentNullException(nameof(persistentStorage));

            this.persistentStorage = persistentStorage;
        }


        public async Task PublishPackage(Stream stream)
        {
            using (NuGet.Packaging.PackageArchiveReader packageArchiveReader = new NuGet.Packaging.PackageArchiveReader(stream))
            {
                var packageName = packageArchiveReader.NuspecReader.GetId().ToLower();
                var version = packageArchiveReader.NuspecReader.GetVersion().ToNormalizedString();

                await this.persistentStorage.WriteContent($"{packageName}/{version}/{packageName}.{version}.nupkg", stream).ConfigureAwait(false);
            }
        }
    }
}
