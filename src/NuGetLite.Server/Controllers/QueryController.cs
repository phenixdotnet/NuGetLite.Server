using Microsoft.AspNetCore.Mvc;
using NuGetLite.Server.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Controllers
{
    [Route("query")]
    public class QueryController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get(string q = "", int skip = 0, int take = 100, bool prerelease = false, string semVerLevel = "2.0.0")
        {
            var result = new QueryResponse();

            return this.Ok(result);
        }

        public class QueryResponse
        {
            /// <summary>
            /// Gets or sets the total hits
            /// </summary>
            public int TotalHits
            { get; set; }

            /// <summary>
            /// Gets or sets the data
            /// </summary>
            public IEnumerable<NuGetPackageSummary> Data
            { get; set; }
        }
    }
}
