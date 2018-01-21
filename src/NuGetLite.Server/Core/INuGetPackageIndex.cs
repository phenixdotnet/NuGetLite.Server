using NuGet.Packaging.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    /// <summary>
    /// Define the contract for a nuget package searcher
    /// </summary>
    public interface INuGetPackageIndex
    {

        /// <summary>
        /// Index a package from its metadata
        /// </summary>
        /// <param name="nuspecReader">The nuspec reader instance to be used to read metadata</param>
        /// <returns></returns>
        Task<RegistrationResult> IndexPackage(INuspecCoreReader nuspecReader);

        /// <summary>
        /// Gets count packages match the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to be used to search the packages</param>
        /// <param name="includePrerelease">A value indicating if the pre release packages should be included or not</param>
        /// <returns></returns>
        Task<int> Count(string query, bool includePrerelease);

        /// <summary>
        /// Return the package summary which match with the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to be used to search the packages</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="includePrerelease">A value indicating if the pre release packages should be included or not</param>
        /// <returns></returns>
        Task<IEnumerable<NuGetPackageSummary>> SearchPackages(string query, int skip, int take, bool includePrerelease);
    }
}
