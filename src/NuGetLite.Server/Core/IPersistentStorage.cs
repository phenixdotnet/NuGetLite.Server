using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core
{
    /// <summary>
    /// Define the contract for a persistent storage
    /// </summary>
    public interface IPersistentStorage
    {
        Task WriteContent(string name, Stream stream);
    }
}
