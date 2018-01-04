using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGetLite.Server.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NuGetLite.Server.Controllers
{
    [Route("/api/v2")]
    public class V2PublishController : Controller
    {
        private readonly NuGetPackageManager publishPackageManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="V2PublishController"/> class
        /// </summary>
        /// <param name="publishPackageManager">The persistent storage implementation to be used</param>
        public V2PublishController(NuGetPackageManager publishPackageManager)
        {
            if (publishPackageManager == null)
                throw new ArgumentNullException(nameof(publishPackageManager));

            this.publishPackageManager = publishPackageManager;
        }

        /// <summary>
        /// Put a package in the store
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put()
        {
            if (!Request.HasFormContentType)
            {
                return this.StatusCode((int)HttpStatusCode.UnsupportedMediaType);
            }

            var form = await Request.ReadFormAsync();
            if (form == null || form.Files.Count != 1)
                return this.StatusCode(500);
            
            using (MemoryStream ms = new MemoryStream())
            {
                await form.Files.First().CopyToAsync(ms);

                await this.publishPackageManager.PublishPackage(ms);
            }

            return this.StatusCode(201);
        }
    }
}
