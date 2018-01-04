using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


        [HttpPut]
        public async Task<IActionResult> Put(ICollection<IFormFile> files)
        {
            if (!Request.HasFormContentType)
            {
                return this.StatusCode((int)HttpStatusCode.UnsupportedMediaType);
            }

            var form = await Request.ReadFormAsync();
            if (form == null || form.Files.Count != 0)
                return this.StatusCode(500);

            string fileName = form.Files.First().FileName;
            using (MemoryStream ms = new MemoryStream())
            {
                await form.Files.First().CopyToAsync(ms);


            }

            return this.Ok();
        }
    }
}
