using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server
{
    public class NuGetContentTypeProvider : IContentTypeProvider
    {
        public bool TryGetContentType(string subpath, out string contentType)
        {
            string extension = Path.GetExtension(subpath);
            if (".json".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                contentType = "application/json";
                return true;
            }
            else if (".nupkg".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                contentType = "application/octet-stream";
                return true;
            }
            else if(".nuspec".Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                contentType = "application/xml";
                return true;
            }

            contentType = string.Empty;
            return false;
        }
    }
}
