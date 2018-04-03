using Microsoft.AspNetCore.Mvc;
using NuGetLite.Server.Core;
using NuGetLite.Server.Models;
using System;
using System.Threading.Tasks;

namespace NuGetLite.Server.Controllers
{
    [Route("query")]
    public class QueryController : Controller
    {
        private readonly NuGetPackageManager packageManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryController"/> class
        /// </summary>
        /// <param name="packageManager"></param>
        public QueryController(NuGetPackageManager packageManager)
        {
            if (packageManager == null)
                throw new ArgumentNullException(nameof(packageManager));

            this.packageManager = packageManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string q = "", int skip = 0, int take = 100, bool prerelease = false, string semVerLevel = "2.0.0")
        {
            var searchResult = await this.packageManager.Search(q, skip, take, prerelease);
            var result = new QueryResponse(searchResult.Item1, searchResult.Item2);

            return this.Ok(result);
        }

    }
}
