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
        /// <summary>
        /// Load the content of the file <paramref name="name"/> and deserialize it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<T> LoadContent<T>(string name);
        
        /// <summary>
        /// Write the <paramref name="obj"/> to <paramref name="name"/>
        /// </summary>
        /// <param name="name">The file name to be used</param>
        /// <param name="obj">The object which should be serialized and wrote</param>
        /// <returns></returns>
        Task WriteContent(string name, object obj);

        /// <summary>
        /// Write the <paramref name="stream"/> to <paramref name="name"/>
        /// </summary>
        /// <param name="name">The file name to be used</param>
        /// <param name="stream">The stream which should be used</param>
        /// <returns></returns>
        Task WriteContent(string name, Stream stream);
    }
}
