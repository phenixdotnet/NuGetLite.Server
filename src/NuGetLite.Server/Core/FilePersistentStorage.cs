using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    public class FilePersistentStorage : IPersistentStorage
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };

        private readonly string storageBasePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePersistentStorage"/> class
        /// </summary>
        /// <param name="basePath">The physical base path to be used</param>
        public FilePersistentStorage(string basePath)
        {
            this.storageBasePath = Path.GetFullPath(basePath);
        }

        public Task<T> LoadContent<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            string filePath = this.GetFullPathFromFile(name).ToLowerInvariant();
            if (!File.Exists(filePath))
                return Task.FromResult<T>(default(T));

            var jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);

            using (TextReader tr = new StreamReader(filePath))
            using (JsonReader jsonReader = new JsonTextReader(tr))
            {
                return Task.FromResult(jsonSerializer.Deserialize<T>(jsonReader));
            }
        }

        public async Task WriteContent(string name, object obj)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var jsonSerializer = JsonSerializer.Create(jsonSerializerSettings);
            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms))
            using (JsonWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonSerializer.Serialize(jsonWriter, obj);

                jsonWriter.Flush();
                sw.Flush();

                ms.Seek(0, SeekOrigin.Begin);
                await this.WriteContent(name, ms).ConfigureAwait(false);
            }
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

            if (!directory.StartsWith(this.storageBasePath, StringComparison.OrdinalIgnoreCase))
                throw new Exception();

            string fileName = Path.GetFileName(name);

            return Path.Combine(directory, fileName);
        }
    }
}
