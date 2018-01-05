using Microsoft.AspNetCore.Mvc;
using NuGetLite.Server.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Controllers
{
    public class RegistrationController : Controller
    {

        /// <summary>
        /// Gets the package index
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/registration/{packageId}/index.json")]
        public async Task<IActionResult> GetIndex(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
                throw new ArgumentNullException(nameof(packageId));

            var result = new RegistrationResult();

            return this.Ok(result);
        }

        
    }
}
