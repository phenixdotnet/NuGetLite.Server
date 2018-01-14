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

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePersistentStorage"/> class
        /// </summary>
        public FilePersistentStorage()
        {
            this.storageBasePath = Path.GetFullPath("./packages");
        }
        
        public async Task WriteContent(string name, Stream stream)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));


            string filePath = this.GetFullPathFromFile(name).ToLowerInvariant();
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                int countRead = 0;
                byte[] buffer = new byte[2048];
                do
                {
                    countRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    await fs.WriteAsync(buffer, 0, countRead).ConfigureAwait(false);
                }
                while (countRead == buffer.Length);
            }
        }

        private string GetFullPathFromFile(string name)
        {
            string directory = Path.GetDirectoryName(name);
            directory = Path.GetFullPath(Path.Combine(storageBasePath, directory));

            if (!directory.StartsWith(this.storageBasePath))
                throw new Exception();

            string fileName = Path.GetFileName(name);

            return Path.Combine(directory, fileName);
        }
    }
}
