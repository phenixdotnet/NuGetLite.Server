using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace NuGetLite.Server.Core.PackageIndex
{
    public class PackageVersionAlreadyExistsException : Exception
    {
        public PackageVersionAlreadyExistsException()
        {
        }

        public PackageVersionAlreadyExistsException(string message) : base(message)
        {
        }

        public PackageVersionAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PackageVersionAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
