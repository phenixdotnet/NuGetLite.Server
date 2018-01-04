using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class FilePersistentStorage : IPersistentStorage
    {
        private readonly string storageBasePath;

        public async Task WriteContent(string name, Stream stream)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));


            string directory = this.GetFullPathFromFile(name);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fs = new FileStream(directory, FileMode.Create))
            {
                int countRead = 0;
                byte[] buffer = new byte[2048];
                while ((countRead = await stream.ReadAsync(buffer, 0, buffer.Length)) == buffer.Length)
                {
                    await fs.WriteAsync(buffer, 0, countRead);
                }
            }
        }

        private string GetFullPathFromFile(string name)
        {
            string directory = Path.GetDirectoryName(name);
            directory = Path.GetFullPath(Path.Combine(storageBasePath, directory));

            if (!directory.StartsWith(this.storageBasePath))
                throw new Exception();

            return directory;
        }
    }
}
