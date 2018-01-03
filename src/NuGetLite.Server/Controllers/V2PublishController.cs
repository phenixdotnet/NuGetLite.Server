using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Controllers
{
    
    public class V2PublishController : ControllerBase
    {
        [HttpPut]
        public async Task<IActionResult> Put()
        {
            throw new NotImplementedException();
        }
    }
}
