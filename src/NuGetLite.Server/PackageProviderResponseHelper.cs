using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using NuGetLite.Server.Core.PackageIndex;
using System;
using System.IO;

namespace NuGetLite.Server
{
    public static class PackageProviderResponseHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="staticFileResponseContext"></param>
        public static void OnPrepareResponse(StaticFileResponseContext staticFileResponseContext)
        {
            if (staticFileResponseContext == null || 
                staticFileResponseContext.File == null || 
                (!".nuspec".Equals(Path.GetExtension(staticFileResponseContext.File.Name), StringComparison.OrdinalIgnoreCase) && !".nupkg".Equals(Path.GetExtension(staticFileResponseContext.File.Name), StringComparison.OrdinalIgnoreCase)))
                return;

            var nuGetPackageIndex = staticFileResponseContext.Context.RequestServices.GetService<INuGetPackageIndex>();
            nuGetPackageIndex.IncrementDownloadCounter(staticFileResponseContext.File.PhysicalPath);
        }
    }
}
